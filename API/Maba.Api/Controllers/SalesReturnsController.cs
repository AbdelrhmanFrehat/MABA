using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Catalog;
using Maba.Domain.CommercialReturns;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/sales-returns")]
[Authorize]
public class SalesReturnsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public SalesReturnsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalesReturnDto>>> GetSalesReturns(CancellationToken cancellationToken)
    {
        var rows = await _context.Set<SalesReturn>()
            .Include(x => x.Lines)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(rows.Select(ToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SalesReturnDto>> GetSalesReturn(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<SalesReturn>()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity == null ? NotFound() : Ok(ToDto(entity));
    }

    [HttpPost]
    public async Task<ActionResult<SalesReturnDto>> CreateSalesReturn([FromBody] CreateSalesReturnRequest request, CancellationToken cancellationToken)
    {
        if (request.Lines == null || request.Lines.Count == 0)
            return BadRequest("At least one return line is required.");

        var entity = new SalesReturn
        {
            ReturnNumber = await GenerateNumberAsync("SR", cancellationToken),
            CustomerId = request.CustomerId,
            CustomerName = request.CustomerName ?? "Customer",
            StatusKey = "draft",
            StatusName = "Draft",
            StatusColor = "#64748b",
            SalesInvoiceId = request.SalesInvoiceId,
            SalesInvoiceNumber = request.SalesInvoiceNumber,
            ReturnReasonLookupId = request.ReturnReasonLookupId,
            ReturnReasonName = request.ReturnReasonName,
            ReturnDate = request.ReturnDate,
            Currency = request.Currency ?? "ILS",
            RestockItems = request.RestockItems ?? true,
            Notes = request.Notes,
            WarehouseId = request.WarehouseId,
            CreatedByUserId = GetCurrentUserId(),
            Lines = request.Lines.Select((line, index) => new SalesReturnLine
            {
                SalesInvoiceLineId = line.SalesInvoiceLineId,
                ItemId = line.ItemId,
                ItemName = line.ItemName,
                ItemSku = line.ItemSku,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                DiscountAmount = line.DiscountAmount,
                TaxAmount = line.TaxAmount,
                LineTotal = line.LineTotal != 0 ? line.LineTotal : (line.Quantity * line.UnitPrice) - line.DiscountAmount + line.TaxAmount,
                Notes = line.Notes
            }).ToList()
        };

        RecalculateTotals(entity);

        _context.Set<SalesReturn>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetSalesReturn), new { id = entity.Id }, ToDto(entity));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SalesReturnDto>> UpdateSalesReturn(Guid id, [FromBody] CreateSalesReturnRequest request, CancellationToken cancellationToken)
    {
        if (request.Lines == null || request.Lines.Count == 0)
            return BadRequest("At least one return line is required.");

        var entity = await _context.Set<SalesReturn>()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity == null) return NotFound();
        if (entity.IsPosted) return BadRequest("Completed sales returns cannot be edited.");

        entity.CustomerId = request.CustomerId;
        entity.CustomerName = request.CustomerName ?? entity.CustomerName;
        entity.SalesInvoiceId = request.SalesInvoiceId;
        entity.SalesInvoiceNumber = request.SalesInvoiceNumber;
        entity.ReturnReasonLookupId = request.ReturnReasonLookupId;
        entity.ReturnReasonName = request.ReturnReasonName;
        entity.ReturnDate = request.ReturnDate;
        entity.Currency = request.Currency ?? entity.Currency;
        entity.RestockItems = request.RestockItems ?? entity.RestockItems;
        entity.Notes = request.Notes;
        entity.WarehouseId = request.WarehouseId;

        _context.Set<SalesReturnLine>().RemoveRange(entity.Lines);
        entity.Lines = request.Lines.Select(line => new SalesReturnLine
        {
            SalesInvoiceLineId = line.SalesInvoiceLineId,
            ItemId = line.ItemId,
            ItemName = line.ItemName,
            ItemSku = line.ItemSku,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,
            DiscountAmount = line.DiscountAmount,
            TaxAmount = line.TaxAmount,
            LineTotal = line.LineTotal != 0 ? line.LineTotal : (line.Quantity * line.UnitPrice) - line.DiscountAmount + line.TaxAmount,
            Notes = line.Notes
        }).ToList();

        RecalculateTotals(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(entity));
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult> ApproveSalesReturn(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<SalesReturn>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null) return NotFound();

        entity.StatusKey = "approved";
        entity.StatusName = "Approved";
        entity.StatusColor = "#22c55e";
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult> CompleteSalesReturn(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<SalesReturn>()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity == null) return NotFound();
        if (entity.IsPosted) return NoContent();

        if (entity.RestockItems)
            await ApplySalesReturnInventoryAsync(entity, cancellationToken);

        entity.StatusKey = "completed";
        entity.StatusName = "Completed";
        entity.StatusColor = "#10b981";
        entity.IsPosted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteSalesReturn(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<SalesReturn>()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity == null) return NotFound();
        if (entity.IsPosted) return BadRequest("Completed sales returns cannot be deleted.");

        _context.Set<SalesReturn>().Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task ApplySalesReturnInventoryAsync(SalesReturn entity, CancellationToken cancellationToken)
    {
        foreach (var line in entity.Lines.Where(x => x.ItemId != Guid.Empty))
        {
            var inventory = await _context.Set<Inventory>()
                .FirstOrDefaultAsync(x => x.ItemId == line.ItemId && x.WarehouseId == entity.WarehouseId, cancellationToken);

            if (inventory == null)
            {
                inventory = new Inventory
                {
                    ItemId = line.ItemId,
                    WarehouseId = entity.WarehouseId,
                    QuantityOnHand = 0,
                    ReorderLevel = 0
                };
                _context.Set<Inventory>().Add(inventory);
            }

            inventory.QuantityOnHand += Decimal.ToInt32(line.Quantity);
            inventory.LastStockInAt = DateTime.UtcNow;

            _context.Set<InventoryTransaction>().Add(new InventoryTransaction
            {
                Inventory = inventory,
                TransactionType = "StockIn",
                Quantity = Decimal.ToInt32(line.Quantity),
                WarehouseId = entity.WarehouseId,
                DocumentType = "SalesReturn",
                DocumentId = entity.Id,
                Reason = "Sales return restock",
                Notes = entity.Notes,
                CreatedByUserId = entity.CreatedByUserId
            });
        }
    }

    private async Task<string> GenerateNumberAsync(string prefix, CancellationToken cancellationToken)
    {
        var count = await _context.Set<SalesReturn>().CountAsync(cancellationToken) + 1;
        return $"{prefix}-{DateTime.UtcNow:yyyy}-{count:D4}";
    }

    private static void RecalculateTotals(SalesReturn entity)
    {
        entity.SubTotal = entity.Lines.Sum(x => x.Quantity * x.UnitPrice);
        entity.TaxAmount = entity.Lines.Sum(x => x.TaxAmount);
        entity.Total = entity.Lines.Sum(x => x.LineTotal);
    }

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }

    private static SalesReturnDto ToDto(SalesReturn x) => new()
    {
        Id = x.Id,
        ReturnNumber = x.ReturnNumber,
        CustomerId = x.CustomerId,
        CustomerName = x.CustomerName,
        StatusLookupId = x.StatusKey,
        StatusName = x.StatusName,
        StatusColor = x.StatusColor,
        SalesInvoiceId = x.SalesInvoiceId,
        SalesInvoiceNumber = x.SalesInvoiceNumber,
        ReturnReasonLookupId = x.ReturnReasonLookupId,
        ReturnReasonName = x.ReturnReasonName,
        ReturnDate = x.ReturnDate,
        Currency = x.Currency,
        SubTotal = x.SubTotal,
        TaxAmount = x.TaxAmount,
        Total = x.Total,
        IsPosted = x.IsPosted,
        RestockItems = x.RestockItems,
        Notes = x.Notes,
        WarehouseId = x.WarehouseId,
        CreatedByUserId = x.CreatedByUserId,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
        Lines = x.Lines.Select(l => new SalesReturnLineDto
        {
            Id = l.Id,
            SalesInvoiceLineId = l.SalesInvoiceLineId,
            ItemId = l.ItemId,
            ItemName = l.ItemName,
            ItemSku = l.ItemSku,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice,
            DiscountAmount = l.DiscountAmount,
            TaxAmount = l.TaxAmount,
            LineTotal = l.LineTotal,
            Notes = l.Notes
        }).ToList()
    };

    public class CreateSalesReturnRequest
    {
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public Guid? SalesInvoiceId { get; set; }
        public string? SalesInvoiceNumber { get; set; }
        public string? ReturnReasonLookupId { get; set; }
        public string? ReturnReasonName { get; set; }
        public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
        public string? Currency { get; set; }
        public bool? RestockItems { get; set; }
        public string? Notes { get; set; }
        public Guid? WarehouseId { get; set; }
        public List<CreateSalesReturnLineRequest> Lines { get; set; } = new();
    }

    public class CreateSalesReturnLineRequest
    {
        public Guid? SalesInvoiceLineId { get; set; }
        public Guid ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? ItemSku { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public string? Notes { get; set; }
    }

    public class SalesReturnDto
    {
        public Guid Id { get; set; }
        public string ReturnNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string StatusLookupId { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string? StatusColor { get; set; }
        public Guid? SalesInvoiceId { get; set; }
        public string? SalesInvoiceNumber { get; set; }
        public string? ReturnReasonLookupId { get; set; }
        public string? ReturnReasonName { get; set; }
        public DateTime ReturnDate { get; set; }
        public string Currency { get; set; } = "ILS";
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public bool IsPosted { get; set; }
        public bool RestockItems { get; set; }
        public string? Notes { get; set; }
        public Guid? WarehouseId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<SalesReturnLineDto> Lines { get; set; } = new();
    }

    public class SalesReturnLineDto
    {
        public Guid Id { get; set; }
        public Guid? SalesInvoiceLineId { get; set; }
        public Guid ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? ItemSku { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public string? Notes { get; set; }
    }
}
