using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Sales.Quotations;
using Maba.Domain.Cnc;
using Maba.Domain.Crm;
using Maba.Domain.Design;
using Maba.Domain.DesignCad;
using Maba.Domain.Laser;
using Maba.Domain.Orders;
using Maba.Domain.Printing;
using Maba.Domain.Projects;
using Maba.Domain.Sales;

namespace Maba.Api.Controllers;

/// <summary>
/// Commercial quotation pipeline: Request → Quotation → Sales Order → Invoice.
/// Quotations are always linked to an ERP Customer, not a website User directly.
/// </summary>
[ApiController]
[Route("api/v1/sales-quotations")]
[Authorize]
public class SalesQuotationsController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IDocumentNumberService _docNumbers;
    private readonly ILogger<SalesQuotationsController> _logger;

    // Valid workflow statuses that allow quotation creation
    private static readonly HashSet<string> AllowedWorkflowStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "UnderReview", "AwaitingCustomerConfirmation", "Approved", "InProgress", "ReadyForDelivery"
    };

    public SalesQuotationsController(
        IApplicationDbContext context,
        IDocumentNumberService docNumbers,
        ILogger<SalesQuotationsController> logger)
    {
        _context = context;
        _docNumbers = docNumbers;
        _logger = logger;
    }

    // ── GET /api/v1/sales-quotations ─────────────────────────────────────────

    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuotationDto>>> GetAll(
        [FromQuery] Guid? customerId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? sourceRequestType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Quotation>()
            .Include(q => q.Customer)
            .Include(q => q.Items)
            .Include(q => q.ConvertedToOrder)
            .AsNoTracking()
            .AsQueryable();

        if (customerId.HasValue) query = query.Where(q => q.CustomerId == customerId.Value);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<QuotationStatus>(status, true, out var parsedStatus))
            query = query.Where(q => q.Status == parsedStatus);
        if (!string.IsNullOrWhiteSpace(sourceRequestType))
            query = query.Where(q => q.SourceRequestType == sourceRequestType);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(q => MapToDto(q))
            .ToListAsync(cancellationToken);

        Response.Headers["X-Total-Count"] = total.ToString();
        return Ok(items);
    }

    // ── GET /api/v1/sales-quotations/{id} ────────────────────────────────────

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuotationDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var q = await _context.Set<Quotation>()
            .Include(x => x.Customer)
            .Include(x => x.Items.OrderBy(i => i.LineNumber))
            .Include(x => x.ConvertedToOrder)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return q == null ? NotFound() : Ok(MapToDto(q));
    }

    // ── POST /api/v1/sales-quotations ────────────────────────────────────────

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<QuotationDto>> Create(
        [FromBody] CreateQuotationRequest req,
        CancellationToken cancellationToken)
    {
        var customer = await _context.Set<Customer>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == req.CustomerId && c.IsActive, cancellationToken);

        if (customer == null)
            return BadRequest(new { message = "Customer not found or inactive." });

        var quotation = await BuildQuotation(req.CustomerId, req.QuotationDate, req.ValidUntil,
            req.Currency, req.Notes, req.InternalNotes, req.TermsAndConditions,
            req.Items, null, null, null, cancellationToken);

        _context.Set<Quotation>().Add(quotation);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = quotation.Id },
            await GetByIdDto(quotation.Id, cancellationToken));
    }

    // ── POST /api/v1/sales-quotations/from-request ───────────────────────────

    [HttpPost("from-request")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<QuotationDto>> CreateFromRequest(
        [FromBody] CreateQuotationFromRequestRequest req,
        CancellationToken cancellationToken)
    {
        var normalized = NormalizeRequestType(req.RequestType);
        if (normalized == null)
            return BadRequest(new { message = $"Unknown request type: {req.RequestType}" });

        // Load request info + validate state
        var info = await LoadRequestInfo(normalized, req.RequestId, cancellationToken);
        if (info == null)
            return NotFound(new { message = "Service request not found." });

        if (!AllowedWorkflowStatuses.Contains(info.WorkflowStatus))
            return BadRequest(new { message = $"Cannot create quotation for a request in '{info.WorkflowStatus}' status. Allowed: {string.Join(", ", AllowedWorkflowStatuses)}." });

        if (info.CustomerId == null)
            return BadRequest(new { message = "No ERP customer linked to this request. Please resolve customer linking first." });

        // Prevent duplicate active quotation (Draft/Sent/Accepted)
        var existingActive = await _context.Set<Quotation>()
            .AsNoTracking()
            .Where(q => q.SourceRequestId == req.RequestId
                     && q.SourceRequestType == normalized
                     && (q.Status == QuotationStatus.Draft || q.Status == QuotationStatus.Sent || q.Status == QuotationStatus.Accepted))
            .FirstOrDefaultAsync(cancellationToken);

        if (existingActive != null)
            return Conflict(new
            {
                message = "An active quotation already exists for this request.",
                existingQuotationId = existingActive.Id,
                existingQuotationNumber = existingActive.QuotationNumber,
                status = existingActive.Status.ToString()
            });

        var createdByUserId = GetCurrentUserId();
        var quotation = await BuildQuotation(
            info.CustomerId.Value,
            null, req.ValidUntil, req.Currency,
            req.Notes, req.InternalNotes, req.TermsAndConditions,
            req.Items, req.RequestId, normalized, info.ReferenceNumber,
            cancellationToken);

        quotation.CreatedByUserId = createdByUserId;

        _context.Set<Quotation>().Add(quotation);

        // Back-link the request to this quotation
        await LinkQuotationToRequest(normalized, req.RequestId, quotation.Id, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Quotation {Number} created from {Type} request {Id}",
            quotation.QuotationNumber, normalized, req.RequestId);

        return CreatedAtAction(nameof(GetById), new { id = quotation.Id },
            await GetByIdDto(quotation.Id, cancellationToken));
    }

    // ── PUT /api/v1/sales-quotations/{id} ────────────────────────────────────

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<QuotationDto>> Update(
        Guid id,
        [FromBody] UpdateQuotationRequest req,
        CancellationToken cancellationToken)
    {
        var quotation = await _context.Set<Quotation>()
            .Include(q => q.Items)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (quotation == null) return NotFound();

        if (quotation.Status == QuotationStatus.Converted)
            return BadRequest(new { message = "Cannot edit a quotation that has already been converted to a sales order." });

        if (req.ValidUntil.HasValue) quotation.ValidUntil = req.ValidUntil;
        if (req.Notes != null) quotation.Notes = req.Notes;
        if (req.InternalNotes != null) quotation.InternalNotes = req.InternalNotes;
        if (req.TermsAndConditions != null) quotation.TermsAndConditions = req.TermsAndConditions;

        if (req.Status != null && Enum.TryParse<QuotationStatus>(req.Status, true, out var newStatus)
            && quotation.Status != QuotationStatus.Converted)
        {
            quotation.Status = newStatus;
        }

        if (req.Items != null)
        {
            // Replace line items
            _context.Set<QuotationItem>().RemoveRange(quotation.Items);
            var newItems = BuildItems(req.Items);
            foreach (var item in newItems) item.QuotationId = quotation.Id;
            _context.Set<QuotationItem>().AddRange(newItems);
            quotation.Items = newItems;
            RecalcTotals(quotation);
        }

        quotation.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(await GetByIdDto(id, cancellationToken));
    }

    // ── POST /api/v1/sales-quotations/{id}/send ──────────────────────────────

    [HttpPost("{id:guid}/send")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> Send(Guid id, CancellationToken cancellationToken)
    {
        var quotation = await _context.Set<Quotation>().FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
        if (quotation == null) return NotFound();
        if (quotation.Status != QuotationStatus.Draft)
            return BadRequest(new { message = "Only Draft quotations can be sent." });

        quotation.Status = QuotationStatus.Sent;
        quotation.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // ── POST /api/v1/sales-quotations/{id}/accept ────────────────────────────

    [HttpPost("{id:guid}/accept")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> Accept(Guid id, CancellationToken cancellationToken)
    {
        var quotation = await _context.Set<Quotation>().FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
        if (quotation == null) return NotFound();
        if (quotation.Status != QuotationStatus.Sent && quotation.Status != QuotationStatus.Draft)
            return BadRequest(new { message = "Only Draft or Sent quotations can be accepted." });

        quotation.Status = QuotationStatus.Accepted;
        quotation.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // ── POST /api/v1/sales-quotations/{id}/reject ────────────────────────────

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> Reject(Guid id, CancellationToken cancellationToken)
    {
        var quotation = await _context.Set<Quotation>().FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
        if (quotation == null) return NotFound();
        if (quotation.Status == QuotationStatus.Converted)
            return BadRequest(new { message = "Cannot reject a converted quotation." });

        quotation.Status = QuotationStatus.Rejected;
        quotation.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // ── POST /api/v1/sales-quotations/{id}/convert-to-order ─────────────────

    [HttpPost("{id:guid}/convert-to-order")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<SalesOrdersController.SalesOrderDto>> ConvertToOrder(
        Guid id,
        CancellationToken cancellationToken)
    {
        var quotation = await _context.Set<Quotation>()
            .Include(q => q.Items.OrderBy(i => i.LineNumber))
            .Include(q => q.Customer)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (quotation == null) return NotFound();
        if (quotation.Status == QuotationStatus.Converted)
            return BadRequest(new { message = "This quotation has already been converted to a sales order." });
        if (quotation.Status == QuotationStatus.Rejected || quotation.Status == QuotationStatus.Expired)
            return BadRequest(new { message = $"Cannot convert a {quotation.Status} quotation." });

        // Require Accepted or at least Sent before converting
        if (quotation.Status == QuotationStatus.Draft)
        {
            // Auto-accept on convert
            quotation.Status = QuotationStatus.Accepted;
        }

        // Resolve OrderStatus — find "new" or first available
        var orderStatus = await _context.Set<OrderStatus>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key.ToLower().Contains("new") || s.Key.ToLower().Contains("draft") || s.Key.ToLower().Contains("pending"), cancellationToken)
            ?? await _context.Set<OrderStatus>().AsNoTracking().FirstOrDefaultAsync(cancellationToken);

        if (orderStatus == null)
            return BadRequest(new { message = "No OrderStatus seeded. Please seed order statuses before converting." });

        // Resolve UserId — use customer's linked user or the admin performing the action
        var userId = quotation.Customer?.UserId ?? GetCurrentUserId() ?? Guid.Empty;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = await _docNumbers.GenerateNextAsync("Order", cancellationToken),
            IsStorefrontOrder = false,
            UserId = userId,
            CustomerId = quotation.CustomerId,
            OrderStatusId = orderStatus.Id,
            SourceQuotationId = quotation.Id,
            SourceRequestId = quotation.SourceRequestId,
            SourceRequestType = quotation.SourceRequestType,
            SourceRequestReference = quotation.SourceRequestReference,
            SubTotal = quotation.SubTotal,
            DiscountAmount = quotation.DiscountAmount,
            TaxAmount = quotation.TaxAmount,
            Total = quotation.Total,
            Currency = quotation.Currency,
            Notes = quotation.Notes,
            CreatedAt = DateTime.UtcNow
        };

        // Map quotation items → order items (free-text service lines, no catalog item)
        var orderItems = quotation.Items.Select((qi, idx) => new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            ItemId = null,
            MetaJson = SerializeOrderItemMeta(qi.Description, qi.Unit, qi.Notes),
            Quantity = Math.Max(1, (int)Math.Ceiling(qi.Quantity)),
            UnitPrice = qi.UnitPrice,
            DiscountAmount = qi.DiscountAmount,
            TaxAmount = qi.TaxAmount,
            LineTotal = qi.LineTotal,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        order.OrderItems = orderItems;

        _context.Set<Order>().Add(order);
        _context.Set<OrderItem>().AddRange(orderItems);

        // Mark quotation as converted
        quotation.Status = QuotationStatus.Converted;
        quotation.ConvertedToOrderId = order.Id;
        quotation.ConvertedAt = DateTime.UtcNow;
        quotation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Quotation {QNum} converted to Order {ONum}", quotation.QuotationNumber, order.OrderNumber);

        // Return the new order DTO
        var createdOrder = await _context.Set<Order>()
            .Include(o => o.User)
            .Include(o => o.OrderStatus)
            .Include(o => o.OrderItems)
            .Include(o => o.Customer)
            .AsNoTracking()
            .FirstAsync(o => o.Id == order.Id, cancellationToken);

        return Ok(MapOrderToDto(createdOrder));
    }

    // ── GET /api/v1/sales-quotations/commercial-draft/{requestType}/{requestId} ─

    [HttpGet("commercial-draft/{requestType}/{requestId:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<RequestCommercialDraftDto>> GetCommercialDraft(
        string requestType, Guid requestId, CancellationToken cancellationToken)
    {
        var normalized = NormalizeRequestType(requestType);
        if (normalized == null) return BadRequest(new { message = $"Unknown request type: {requestType}" });

        var info = await LoadRequestInfo(normalized, requestId, cancellationToken);
        if (info == null) return NotFound();

        // Check for existing active quotation
        var existingQuotation = await _context.Set<Quotation>()
            .AsNoTracking()
            .Where(q => q.SourceRequestId == requestId && q.SourceRequestType == normalized
                     && (q.Status == QuotationStatus.Draft || q.Status == QuotationStatus.Sent || q.Status == QuotationStatus.Accepted))
            .OrderByDescending(q => q.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        string? customerName = null;
        string? customerEmail = null;
        if (info.CustomerId.HasValue)
        {
            var customer = await _context.Set<Customer>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == info.CustomerId, cancellationToken);
            customerName = customer?.NameEn ?? info.CustomerName;
            customerEmail = customer?.Email ?? info.CustomerEmail;
        }

        var canCreate = info.CustomerId.HasValue && AllowedWorkflowStatuses.Contains(info.WorkflowStatus);
        var blockedReason = !info.CustomerId.HasValue
            ? "No ERP customer linked to this request."
            : !AllowedWorkflowStatuses.Contains(info.WorkflowStatus)
                ? $"Request must be in an active review state. Current status: {info.WorkflowStatus}."
                : null;

        return Ok(new RequestCommercialDraftDto
        {
            CustomerId = info.CustomerId,
            CustomerName = customerName ?? info.CustomerName,
            CustomerEmail = customerEmail ?? info.CustomerEmail,
            Notes = info.NotesForQuotation,
            InternalNotes = info.InternalNotes,
            CanCreateQuotation = canCreate,
            BlockedReason = blockedReason,
            HasExistingQuotation = existingQuotation != null,
            ExistingQuotationId = existingQuotation?.Id,
            ExistingQuotationNumber = existingQuotation?.QuotationNumber,
            ExistingQuotationStatus = existingQuotation?.Status.ToString(),
            DefaultItems = info.DefaultItems
        });
    }

    // ── GET /api/v1/sales-quotations/commercial-links/{requestType}/{requestId} ─

    [HttpGet("commercial-links/{requestType}/{requestId:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<RequestCommercialLinksDto>> GetCommercialLinks(
        string requestType, Guid requestId, CancellationToken cancellationToken)
    {
        var normalized = NormalizeRequestType(requestType);
        if (normalized == null) return BadRequest();

        var info = await LoadRequestInfo(normalized, requestId, cancellationToken);
        if (info == null) return NotFound();

        // All quotations for this request
        var quotations = await _context.Set<Quotation>()
            .AsNoTracking()
            .Where(q => q.SourceRequestId == requestId && q.SourceRequestType == normalized)
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => new CommercialDocLinkDto
            {
                Id = q.Id,
                DocumentNumber = q.QuotationNumber,
                Status = q.Status.ToString(),
                StatusColor = MapQuotationStatusColor(q.Status),
                Total = q.Total,
                Currency = q.Currency,
                CreatedAt = q.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // Orders from those quotations or directly linked
        var quotationIds = quotations.Select(q => q.Id).ToList();
        var orders = await _context.Set<Order>()
            .Include(o => o.OrderStatus)
            .AsNoTracking()
            .Where(o => (o.SourceRequestId == requestId && o.SourceRequestType == normalized)
                     || (o.SourceQuotationId != null && quotationIds.Contains(o.SourceQuotationId.Value)))
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new CommercialDocLinkDto
            {
                Id = o.Id,
                DocumentNumber = o.OrderNumber,
                Status = o.OrderStatus != null ? o.OrderStatus.NameEn : "Unknown",
                StatusColor = "#64748b",
                Total = o.Total,
                Currency = o.Currency,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // Invoices linked to those orders
        var orderIds = orders.Select(o => o.Id).ToList();
        var invoices = await _context.Set<Invoice>()
            .Include(i => i.InvoiceStatus)
            .AsNoTracking()
            .Where(i => orderIds.Contains(i.OrderId))
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new CommercialDocLinkDto
            {
                Id = i.Id,
                DocumentNumber = i.InvoiceNumber,
                Status = i.InvoiceStatus != null ? i.InvoiceStatus.NameEn : "Unknown",
                StatusColor = "#64748b",
                Total = i.Total,
                Currency = i.Currency,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var canCreate = info.CustomerId.HasValue && AllowedWorkflowStatuses.Contains(info.WorkflowStatus);
        var hasActiveQuotation = quotations.Any(q =>
            q.Status == "Draft" || q.Status == "Sent" || q.Status == "Accepted");

        string? customerName = null;
        if (info.CustomerId.HasValue)
        {
            customerName = await _context.Set<Customer>()
                .AsNoTracking()
                .Where(c => c.Id == info.CustomerId.Value)
                .Select(c => c.NameEn)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return Ok(new RequestCommercialLinksDto
        {
            RequestId = requestId,
            RequestType = normalized,
            ReferenceNumber = info.ReferenceNumber,
            CustomerId = info.CustomerId,
            CustomerName = customerName ?? info.CustomerName,
            CanCreateQuotation = canCreate && !hasActiveQuotation,
            BlockedReason = !canCreate
                ? (!info.CustomerId.HasValue
                    ? "No ERP customer linked."
                    : $"Request status '{info.WorkflowStatus}' does not allow quotation creation.")
                : (hasActiveQuotation ? "An active quotation already exists." : null),
            Quotations = quotations,
            Orders = orders,
            Invoices = invoices
        });
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<Quotation> BuildQuotation(
        Guid customerId,
        DateTime? quotationDate,
        DateTime? validUntil,
        string currency,
        string? notes,
        string? internalNotes,
        string? terms,
        List<QuotationItemRequest> itemRequests,
        Guid? sourceRequestId,
        string? sourceRequestType,
        string? sourceRequestReference,
        CancellationToken ct)
    {
        var quotNumber = await _docNumbers.GenerateNextAsync("Quotation", ct);
        var items = BuildItems(itemRequests);

        var quotation = new Quotation
        {
            Id = Guid.NewGuid(),
            QuotationNumber = quotNumber,
            CustomerId = customerId,
            Status = QuotationStatus.Draft,
            QuotationDate = quotationDate ?? DateTime.UtcNow,
            ValidUntil = validUntil,
            Currency = currency,
            Notes = notes,
            InternalNotes = internalNotes,
            TermsAndConditions = terms,
            SourceRequestId = sourceRequestId,
            SourceRequestType = sourceRequestType,
            SourceRequestReference = sourceRequestReference,
            CreatedByUserId = GetCurrentUserId(),
            Items = items,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in items) item.QuotationId = quotation.Id;
        RecalcTotals(quotation);
        return quotation;
    }

    private static List<QuotationItem> BuildItems(List<QuotationItemRequest> requests)
    {
        return requests.Select((r, idx) =>
        {
            var discAmt = r.UnitPrice * r.Quantity * (r.DiscountPercent / 100m);
            var afterDisc = r.UnitPrice * r.Quantity - discAmt;
            var taxAmt = afterDisc * (r.TaxPercent / 100m);
            var total = afterDisc + taxAmt;
            return new QuotationItem
            {
                Id = Guid.NewGuid(),
                LineNumber = idx + 1,
                Description = r.Description,
                Quantity = r.Quantity,
                Unit = r.Unit,
                UnitPrice = r.UnitPrice,
                DiscountPercent = r.DiscountPercent,
                DiscountAmount = discAmt,
                TaxPercent = r.TaxPercent,
                TaxAmount = taxAmt,
                LineTotal = total,
                Notes = r.Notes
            };
        }).ToList();
    }

    private static void RecalcTotals(Quotation q)
    {
        q.SubTotal = q.Items.Sum(i => i.UnitPrice * i.Quantity);
        q.DiscountAmount = q.Items.Sum(i => i.DiscountAmount);
        q.TaxAmount = q.Items.Sum(i => i.TaxAmount);
        q.Total = q.Items.Sum(i => i.LineTotal);
    }

    private async Task<QuotationDto> GetByIdDto(Guid id, CancellationToken ct)
    {
        var q = await _context.Set<Quotation>()
            .Include(x => x.Customer)
            .Include(x => x.Items.OrderBy(i => i.LineNumber))
            .Include(x => x.ConvertedToOrder)
            .AsNoTracking()
            .FirstAsync(x => x.Id == id, ct);
        return MapToDto(q);
    }

    private static QuotationDto MapToDto(Quotation q) => new()
    {
        Id = q.Id,
        QuotationNumber = q.QuotationNumber,
        CustomerId = q.CustomerId,
        CustomerName = q.Customer?.NameEn,
        CustomerCode = q.Customer?.Code,
        CustomerEmail = q.Customer?.Email,
        CustomerPhone = q.Customer?.Phone,
        SourceRequestId = q.SourceRequestId,
        SourceRequestType = q.SourceRequestType,
        SourceRequestReference = q.SourceRequestReference,
        Status = q.Status.ToString(),
        StatusColor = MapQuotationStatusColor(q.Status),
        QuotationDate = q.QuotationDate,
        ValidUntil = q.ValidUntil,
        SubTotal = q.SubTotal,
        DiscountAmount = q.DiscountAmount,
        TaxAmount = q.TaxAmount,
        Total = q.Total,
        Currency = q.Currency,
        Notes = q.Notes,
        InternalNotes = q.InternalNotes,
        TermsAndConditions = q.TermsAndConditions,
        ConvertedToOrderId = q.ConvertedToOrderId,
        ConvertedToOrderNumber = q.ConvertedToOrder?.OrderNumber,
        ConvertedAt = q.ConvertedAt,
        CreatedByUserId = q.CreatedByUserId,
        Items = q.Items.Select(i => new QuotationItemDto
        {
            Id = i.Id,
            QuotationId = i.QuotationId,
            LineNumber = i.LineNumber,
            Description = i.Description,
            Quantity = i.Quantity,
            Unit = i.Unit,
            UnitPrice = i.UnitPrice,
            DiscountPercent = i.DiscountPercent,
            DiscountAmount = i.DiscountAmount,
            TaxPercent = i.TaxPercent,
            TaxAmount = i.TaxAmount,
            LineTotal = i.LineTotal,
            Notes = i.Notes
        }).ToList(),
        CreatedAt = q.CreatedAt,
        UpdatedAt = q.UpdatedAt
    };

    private static SalesOrdersController.SalesOrderDto MapOrderToDto(Order o) => new()
    {
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        CustomerId = o.CustomerId ?? o.UserId,
        CustomerName = o.Customer?.NameEn ?? o.User?.FullName ?? o.User?.Email,
        StatusLookupId = o.OrderStatusId,
        StatusName = o.OrderStatus?.NameEn,
        StatusColor = "#f59e0b",
        OrderDate = o.CreatedAt,
        Currency = o.Currency,
        SubTotal = o.SubTotal,
        DiscountAmount = o.DiscountAmount,
        TaxAmount = o.TaxAmount,
        Total = o.Total,
        Notes = o.Notes,
        IsStorefrontOrder = o.IsStorefrontOrder,
        SourceLabel = "Service Order",
        CreatedByUserId = o.UserId,
        Lines = o.OrderItems.Select((i, idx) => new SalesOrdersController.SalesOrderLineDto
        {
            SalesOrderId = o.Id,
            LineNumber = idx + 1,
            ItemId = i.ItemId ?? Guid.Empty,
            ItemName = i.Item?.NameEn ?? ExtractOrderItemDisplayName(i.MetaJson),
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            DiscountAmount = i.DiscountAmount,
            TaxAmount = i.TaxAmount,
            LineTotal = i.LineTotal
        }).ToList(),
        CreatedAt = o.CreatedAt
    };

    private static string MapQuotationStatusColor(QuotationStatus status) => status switch
    {
        QuotationStatus.Draft => "#94a3b8",
        QuotationStatus.Sent => "#3b82f6",
        QuotationStatus.Accepted => "#22c55e",
        QuotationStatus.Rejected => "#ef4444",
        QuotationStatus.Expired => "#f97316",
        QuotationStatus.Converted => "#8b5cf6",
        _ => "#64748b"
    };

    private static string? SerializeOrderItemMeta(string? description, string? unit, string? notes)
    {
        if (string.IsNullOrWhiteSpace(description) && string.IsNullOrWhiteSpace(unit) && string.IsNullOrWhiteSpace(notes))
            return null;

        return JsonSerializer.Serialize(new
        {
            description,
            unit,
            notes
        });
    }

    private static string ExtractOrderItemDisplayName(string? metaJson)
    {
        if (string.IsNullOrWhiteSpace(metaJson))
            return "Service Line";

        try
        {
            using var doc = JsonDocument.Parse(metaJson);
            if (doc.RootElement.TryGetProperty("description", out var description) &&
                description.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(description.GetString()))
            {
                return description.GetString()!;
            }
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(metaJson))
                return metaJson;
        }

        return "Service Line";
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    private static string? NormalizeRequestType(string? t) => t?.ToLowerInvariant() switch
    {
        "project" => "project",
        "cnc" => "cnc",
        "laser" => "laser",
        "print3d" or "3d" or "3dprint" => "print3d",
        "design" => "design",
        "designcad" or "cad" => "designCad",
        _ => null
    };

    // Link quotation back to source request entity
    private async Task LinkQuotationToRequest(string type, Guid requestId, Guid quotationId, CancellationToken ct)
    {
        switch (type)
        {
            case "project":
            {
                var e = await _context.Set<ProjectRequest>().FirstOrDefaultAsync(x => x.Id == requestId, ct);
                if (e != null && e.LinkedQuotationId == null) { e.LinkedQuotationId = quotationId; }
                break;
            }
            case "cnc":
            {
                var e = await _context.Set<CncServiceRequest>().FirstOrDefaultAsync(x => x.Id == requestId, ct);
                if (e != null && e.LinkedQuotationId == null) { e.LinkedQuotationId = quotationId; }
                break;
            }
            case "laser":
            {
                var e = await _context.Set<LaserServiceRequest>().FirstOrDefaultAsync(x => x.Id == requestId, ct);
                if (e != null && e.LinkedQuotationId == null) { e.LinkedQuotationId = quotationId; }
                break;
            }
            case "print3d":
            {
                var e = await _context.Set<Print3dServiceRequest>().FirstOrDefaultAsync(x => x.Id == requestId, ct);
                if (e != null && e.LinkedQuotationId == null) { e.LinkedQuotationId = quotationId; }
                break;
            }
            case "design":
            {
                var e = await _context.Set<DesignServiceRequest>().FirstOrDefaultAsync(x => x.Id == requestId, ct);
                if (e != null && e.LinkedQuotationId == null) { e.LinkedQuotationId = quotationId; }
                break;
            }
            case "designCad":
            {
                var e = await _context.Set<DesignCadServiceRequest>().FirstOrDefaultAsync(x => x.Id == requestId, ct);
                if (e != null && e.LinkedQuotationId == null) { e.LinkedQuotationId = quotationId; }
                break;
            }
        }
    }

    // ── Request info loader ───────────────────────────────────────────────────

    private async Task<RequestInfoDto?> LoadRequestInfo(string type, Guid id, CancellationToken ct)
    {
        switch (type)
        {
            case "project":
            {
                var e = await _context.Set<ProjectRequest>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
                if (e == null) return null;
                var status = Application.Common.ServiceRequests.ServiceRequestWorkflowMapper.FromProject(e.Status).ToString();
                var notes = BuildProjectNotes(e);
                return new RequestInfoDto(id, "project", e.ReferenceNumber, e.CustomerId, e.FullName, e.Email, status,
                    notes, $"Project: {e.FullName} | Domain: {e.MainDomain ?? "N/A"} | Stage: {e.ProjectStage ?? "N/A"}",
                    new List<QuotationItemDraftDto> {
                        new() { Description = "Engineering Project Service", Quantity = 1, Unit = "project", UnitPrice = 0 }
                    });
            }
            case "cnc":
            {
                var e = await _context.Set<CncServiceRequest>().AsNoTracking().Include(x => x.Material).FirstOrDefaultAsync(x => x.Id == id, ct);
                if (e == null) return null;
                var status = Application.Common.ServiceRequests.ServiceRequestWorkflowMapper.FromCnc(e.Status).ToString();
                var notes = BuildCncNotes(e);
                return new RequestInfoDto(id, "cnc", e.ReferenceNumber, e.CustomerId, e.CustomerName, e.CustomerEmail, status,
                    notes, $"CNC {e.ServiceMode} | Material: {e.Material?.NameEn ?? "N/A"} | Qty: {e.Quantity}",
                    new List<QuotationItemDraftDto> {
                        new() { Description = $"CNC {(e.ServiceMode == "pcb" ? "PCB Fabrication" : "Routing")} Service", Quantity = e.Quantity, Unit = "pcs", UnitPrice = 0 }
                    });
            }
            case "laser":
            {
                var e = await _context.Set<LaserServiceRequest>().AsNoTracking().Include(x => x.Material).FirstOrDefaultAsync(x => x.Id == id, ct);
                if (e == null) return null;
                var status = Application.Common.ServiceRequests.ServiceRequestWorkflowMapper.FromLaser(e.Status).ToString();
                var notes = BuildLaserNotes(e);
                return new RequestInfoDto(id, "laser", e.ReferenceNumber, e.CustomerId, e.CustomerName, e.CustomerEmail, status,
                    notes, $"Laser {e.OperationMode} | Material: {e.Material?.NameEn ?? "N/A"} | {e.WidthCm}×{e.HeightCm} cm",
                    new List<QuotationItemDraftDto> {
                        new() { Description = $"Laser {(e.OperationMode == "cut" ? "Cutting" : "Engraving")} Service", Quantity = 1, Unit = "pcs", UnitPrice = 0 }
                    });
            }
            case "print3d":
            {
                var e = await _context.Set<Print3dServiceRequest>().AsNoTracking().Include(x => x.Material).Include(x => x.Profile).FirstOrDefaultAsync(x => x.Id == id, ct);
                if (e == null) return null;
                var status = Application.Common.ServiceRequests.ServiceRequestWorkflowMapper.FromPrint3d(e.Status).ToString();
                var notes = BuildPrint3dNotes(e);
                return new RequestInfoDto(id, "print3d", e.ReferenceNumber, e.CustomerId, e.CustomerName, e.CustomerEmail, status,
                    notes, $"3D Print | Material: {e.Material?.NameEn ?? "N/A"} | Profile: {e.Profile?.NameEn ?? "Standard"} | File: {e.FileName}",
                    new List<QuotationItemDraftDto> {
                        new() { Description = "3D Printing Service", Quantity = 1, Unit = "pcs", UnitPrice = e.SuggestedPrice ?? 0 }
                    });
            }
            case "design":
            {
                var e = await _context.Set<DesignServiceRequest>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
                if (e == null) return null;
                var status = Application.Common.ServiceRequests.ServiceRequestWorkflowMapper.FromDesign(e.Status).ToString();
                var notes = BuildDesignNotes(e);
                return new RequestInfoDto(id, "design", e.ReferenceNumber, e.CustomerId, e.CustomerName, e.CustomerEmail, status,
                    notes, $"Design: {e.Title} | Type: {e.RequestType} | Budget: {e.BudgetRange ?? "N/A"}",
                    new List<QuotationItemDraftDto> {
                        new() { Description = $"Design Service: {e.Title}", Quantity = 1, Unit = "project", UnitPrice = 0 }
                    });
            }
            case "designCad":
            {
                var e = await _context.Set<DesignCadServiceRequest>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
                if (e == null) return null;
                var status = Application.Common.ServiceRequests.ServiceRequestWorkflowMapper.FromDesignCad(e.Status).ToString();
                var notes = BuildCadNotes(e);
                return new RequestInfoDto(id, "designCad", e.ReferenceNumber, e.CustomerId, e.CustomerName, e.CustomerEmail, status,
                    notes, $"CAD: {e.Title} | Type: {e.RequestType} | Process: {e.TargetProcess?.ToString() ?? "N/A"}",
                    new List<QuotationItemDraftDto> {
                        new() { Description = $"CAD Design Service: {e.Title}", Quantity = 1, Unit = "project", UnitPrice = 0 }
                    });
            }
            default: return null;
        }
    }

    // ── Type-specific notes builders ─────────────────────────────────────────

    private static string BuildProjectNotes(ProjectRequest e)
    {
        var lines = new List<string> { $"Reference: {e.ReferenceNumber}", $"Customer: {e.FullName}" };
        if (!string.IsNullOrWhiteSpace(e.ProjectType)) lines.Add($"Project Type: {e.ProjectType}");
        if (!string.IsNullOrWhiteSpace(e.MainDomain)) lines.Add($"Domain: {e.MainDomain}");
        if (!string.IsNullOrWhiteSpace(e.ProjectStage)) lines.Add($"Stage: {e.ProjectStage}");
        if (!string.IsNullOrWhiteSpace(e.BudgetRange)) lines.Add($"Budget Range: {e.BudgetRange}");
        if (!string.IsNullOrWhiteSpace(e.Timeline)) lines.Add($"Timeline: {e.Timeline}");
        if (!string.IsNullOrWhiteSpace(e.Description)) lines.Add($"\nScope:\n{e.Description}");
        return string.Join("\n", lines);
    }

    private static string BuildCncNotes(CncServiceRequest e)
    {
        var lines = new List<string> { $"Reference: {e.ReferenceNumber}", $"Service Mode: {e.ServiceMode?.ToUpper()}" };
        if (e.Material != null) lines.Add($"Material: {e.Material.NameEn}");
        if (e.ServiceMode == "pcb")
        {
            if (!string.IsNullOrWhiteSpace(e.PcbMaterial)) lines.Add($"PCB Material: {e.PcbMaterial}");
            if (e.PcbThickness.HasValue) lines.Add($"Thickness: {e.PcbThickness}mm");
            if (!string.IsNullOrWhiteSpace(e.PcbSide)) lines.Add($"Side: {e.PcbSide}");
            if (!string.IsNullOrWhiteSpace(e.PcbOperation)) lines.Add($"Operation: {e.PcbOperation}");
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(e.OperationType)) lines.Add($"Operation: {e.OperationType}");
            if (e.WidthMm.HasValue) lines.Add($"Dimensions: {e.WidthMm}×{e.HeightMm}×{e.ThicknessMm}mm");
        }
        lines.Add($"Quantity: {e.Quantity}");
        if (!string.IsNullOrWhiteSpace(e.ProjectDescription)) lines.Add($"\nNotes:\n{e.ProjectDescription}");
        return string.Join("\n", lines);
    }

    private static string BuildLaserNotes(LaserServiceRequest e)
    {
        var lines = new List<string> { $"Reference: {e.ReferenceNumber}", $"Operation: {e.OperationMode?.ToUpper()}" };
        if (e.Material != null) lines.Add($"Material: {e.Material.NameEn}");
        if (e.WidthCm.HasValue) lines.Add($"Size: {e.WidthCm}×{e.HeightCm} cm");
        if (!string.IsNullOrWhiteSpace(e.CustomerNotes)) lines.Add($"\nCustomer Notes:\n{e.CustomerNotes}");
        return string.Join("\n", lines);
    }

    private static string BuildPrint3dNotes(Print3dServiceRequest e)
    {
        var lines = new List<string> { $"Reference: {e.ReferenceNumber}", $"File: {e.FileName}" };
        if (e.Material != null) lines.Add($"Material: {e.Material.NameEn}");
        if (e.Profile != null) lines.Add($"Quality Profile: {e.Profile.NameEn}");
        if (e.EstimatedFilamentGrams.HasValue) lines.Add($"Est. Filament: {e.EstimatedFilamentGrams}g");
        if (e.EstimatedPrintTimeHours.HasValue) lines.Add($"Est. Print Time: {e.EstimatedPrintTimeHours}h");
        if (!string.IsNullOrWhiteSpace(e.CustomerNotes)) lines.Add($"\nCustomer Notes:\n{e.CustomerNotes}");
        return string.Join("\n", lines);
    }

    private static string BuildDesignNotes(DesignServiceRequest e)
    {
        var lines = new List<string> { $"Reference: {e.ReferenceNumber}", $"Title: {e.Title}", $"Type: {e.RequestType}" };
        if (!string.IsNullOrWhiteSpace(e.IntendedUse)) lines.Add($"Intended Use: {e.IntendedUse}");
        if (!string.IsNullOrWhiteSpace(e.MaterialPreference)) lines.Add($"Material: {e.MaterialPreference}");
        if (!string.IsNullOrWhiteSpace(e.BudgetRange)) lines.Add($"Budget: {e.BudgetRange}");
        if (!string.IsNullOrWhiteSpace(e.Timeline)) lines.Add($"Timeline: {e.Timeline}");
        if (!string.IsNullOrWhiteSpace(e.Description)) lines.Add($"\nDescription:\n{e.Description}");
        return string.Join("\n", lines);
    }

    private static string BuildCadNotes(DesignCadServiceRequest e)
    {
        var lines = new List<string> { $"Reference: {e.ReferenceNumber}", $"Title: {e.Title}", $"Type: {e.RequestType}" };
        if (e.TargetProcess.HasValue) lines.Add($"Target Process: {e.TargetProcess}");
        if (!string.IsNullOrWhiteSpace(e.IntendedUse)) lines.Add($"Intended Use: {e.IntendedUse}");
        if (!string.IsNullOrWhiteSpace(e.DimensionsNotes)) lines.Add($"Dimensions: {e.DimensionsNotes}");
        if (!string.IsNullOrWhiteSpace(e.Description)) lines.Add($"\nDescription:\n{e.Description}");
        return string.Join("\n", lines);
    }

    private sealed record RequestInfoDto(
        Guid Id, string Type, string ReferenceNumber,
        Guid? CustomerId, string? CustomerName, string? CustomerEmail,
        string WorkflowStatus, string NotesForQuotation, string InternalNotes,
        List<QuotationItemDraftDto> DefaultItems);
}
