using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Accounting.Commands;
using Maba.Domain.Orders;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/sales-invoices")]
[Authorize]
public class SalesInvoicesController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;

    public SalesInvoicesController(IApplicationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalesInvoiceDto>>> GetSalesInvoices(
        [FromQuery] Guid? customerId,
        CancellationToken cancellationToken)
    {
        var query = _context.Set<Invoice>()
            .Include(x => x.Order).ThenInclude(x => x.User)
            .Include(x => x.InvoiceStatus)
            .Include(x => x.Payments)
            .AsQueryable();

        if (customerId.HasValue)
            query = query.Where(x => x.Order.UserId == customerId.Value);

        var invoices = await query
            .OrderByDescending(x => x.IssueDate)
            .Select(x => new SalesInvoiceDto
            {
                Id               = x.Id,
                InvoiceNumber    = x.InvoiceNumber,
                CustomerId       = x.Order.UserId,
                CustomerName     = x.Order.User.FullName ?? x.Order.User.Email,
                StatusLookupId   = x.InvoiceStatusId,
                StatusName       = x.InvoiceStatus.Key,
                SalesOrderId     = x.OrderId,
                SalesOrderNumber = x.Order.OrderNumber,
                InvoiceDate      = x.IssueDate,
                DueDate          = x.DueDate,
                Currency         = x.Currency,
                SubTotal         = x.Order.SubTotal,
                DiscountAmount   = x.Order.DiscountAmount,
                TaxAmount        = x.Order.TaxAmount,
                Total            = x.Total,
                AmountPaid       = x.Payments.Where(p => !p.IsRefunded).Sum(p => p.Amount),
                AmountDue        = x.Total - x.Payments.Where(p => !p.IsRefunded).Sum(p => p.Amount),
                IsPosted         = x.IsPosted,
                PostedAt         = x.PostedAt,
                JournalEntryId   = x.JournalEntryId,
                PdfUrl           = x.PdfUrl,
                CreatedByUserId  = x.Order.UserId,
                CreatedAt        = x.CreatedAt,
                UpdatedAt        = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(invoices);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SalesInvoiceDto>> GetSalesInvoice(Guid id, CancellationToken cancellationToken)
    {
        var x = await _context.Set<Invoice>()
            .Include(i => i.Order).ThenInclude(o => o.User)
            .Include(i => i.InvoiceStatus)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (x == null) return NotFound();

        return Ok(new SalesInvoiceDto
        {
            Id               = x.Id,
            InvoiceNumber    = x.InvoiceNumber,
            CustomerId       = x.Order.UserId,
            CustomerName     = x.Order.User.FullName ?? x.Order.User.Email,
            StatusLookupId   = x.InvoiceStatusId,
            StatusName       = x.InvoiceStatus.Key,
            SalesOrderId     = x.OrderId,
            SalesOrderNumber = x.Order.OrderNumber,
            InvoiceDate      = x.IssueDate,
            DueDate          = x.DueDate,
            Currency         = x.Currency,
            SubTotal         = x.Order.SubTotal,
            DiscountAmount   = x.Order.DiscountAmount,
            TaxAmount        = x.Order.TaxAmount,
            Total            = x.Total,
            AmountPaid       = x.Payments.Where(p => !p.IsRefunded).Sum(p => p.Amount),
            AmountDue        = x.Total - x.Payments.Where(p => !p.IsRefunded).Sum(p => p.Amount),
            IsPosted         = x.IsPosted,
            PostedAt         = x.PostedAt,
            JournalEntryId   = x.JournalEntryId,
            PdfUrl           = x.PdfUrl,
            CreatedByUserId  = x.Order.UserId,
            CreatedAt        = x.CreatedAt,
            UpdatedAt        = x.UpdatedAt
        });
    }

    /// <summary>
    /// POST /api/v1/sales-invoices/{id}/post
    /// Creates the accounting journal entry (DR AR / CR Revenue) and marks the invoice as posted.
    /// Idempotent — returns 400 if already posted.
    /// </summary>
    [HttpPost("{id:guid}/post")]
    public async Task<ActionResult<PostSalesInvoiceResult>> PostInvoice(Guid id, CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        try
        {
            var result = await _mediator.Send(new PostSalesInvoiceCommand
            {
                InvoiceId       = id,
                PostedByUserId  = userId
            }, cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already posted"))
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    public class SalesInvoiceDto
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public Guid StatusLookupId { get; set; }
        public string? StatusName { get; set; }
        public string? StatusColor { get; set; }
        public Guid SalesOrderId { get; set; }
        public string? SalesOrderNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Currency { get; set; } = "ILS";
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountDue { get; set; }
        public bool IsPosted { get; set; }
        public DateTime? PostedAt { get; set; }
        public Guid? JournalEntryId { get; set; }
        public string? PdfUrl { get; set; }
        public string? Notes { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
