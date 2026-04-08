using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Catalog;
using Maba.Domain.CommercialReturns;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/purchase-returns")]
[Authorize]
public class PurchaseReturnsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public PurchaseReturnsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PurchaseReturnDto>>> GetPurchaseReturns(CancellationToken cancellationToken)
    {
        var rows = await _context.Set<PurchaseReturn>()
            .Include(x => x.Lines)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(rows.Select(ToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PurchaseReturnDto>> GetPurchaseReturn(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<PurchaseReturn>()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity == null ? NotFound() : Ok(ToDto(entity));
    }

    [HttpPost]
    public async Task<ActionResult<PurchaseReturnDto>> CreatePurchaseReturn([FromBody] CreatePurchaseReturnRequest request, CancellationToken cancellationToken)
    {
        if (request.Lines == null || request.Lines.Count == 0)
            return BadRequest("At least one return line is required.");

        var entity = new PurchaseReturn
        {
            ReturnNumber = await GenerateNumberAsync("PR", cancellationToken),
            SupplierId = request.SupplierId,
            SupplierName = request.SupplierName ?? "Supplier",
            StatusKey = "draft",
            StatusName = "Draft",
            StatusColor = "#64748b",
            PurchaseInvoiceId = request.PurchaseInvoiceId,
            PurchaseInvoiceNumber = request.PurchaseInvoiceNumber,
            ReturnReasonLookupId = request.ReturnReasonLookupId,
            ReturnReasonName = request.ReturnReasonName,
            ReturnDate = request.ReturnDate,
            Currency = request.Currency ?? "ILS",
            DeductFromInventory = request.DeductFromInventory ?? true,
            Notes = request.Notes,
            WarehouseId = request.WarehouseId,
            CreatedByUserId = GetCurrentUserId(),
            Lines = request.Lines.Select(line => new PurchaseReturnLine
            {
                PurchaseInvoiceLineId = line.PurchaseInvoiceLineId,
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

        _context.Set<PurchaseReturn>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetPurchaseReturn), new { id = entity.Id }, ToDto(entity));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PurchaseReturnDto>> UpdatePurchaseReturn(Guid id, [FromBody] CreatePurchaseReturnRequest request, CancellationToken cancellationToken)
    {
        if (request.Lines == null || request.Lines.Count == 0)
            return BadRequest("At least one return line is required.");

        var entity = await _context.Set<PurchaseReturn>()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity == null) return NotFound();
        if (entity.IsPosted) return BadRequest("Completed purchase returns cannot be edited.");

        entity.SupplierId = request.SupplierId;
        entity.SupplierName = request.SupplierName ?? entity.SupplierName;
        entity.PurchaseInvoiceId = request.PurchaseInvoiceId;
        entity.PurchaseInvoiceNumber = request.PurchaseInvoiceNumber;
        entity.ReturnReasonLookupId = request.ReturnReasonLookupId;
        entity.ReturnReasonName = request.ReturnReasonName;
        entity.ReturnDate = request.ReturnDate;
        entity.Currency = request.Currency ?? entity.Currency;
        entity.DeductFromInventory = request.DeductFromInventory ?? entity.DeductFromInventory;
        entity.Notes = request.Notes;
        entity.WarehouseId = request.WarehouseId;

        _context.Set<PurchaseReturnLine>().RemoveRange(entity.Lines);
        entity.Lines = request.Lines.Select(line => new PurchaseReturnLine
        {
            PurchaseInvoiceLineId = line.PurchaseInvoiceLineId,
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
    public async Task<ActionResult> ApprovePurchaseReturn(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<PurchaseReturn>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null) return NotFound();

        entity.StatusKey = "approved";
        entity.StatusName = "Approved";
        entity.StatusColor = "#22c55e";
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult> CompletePurchaseReturn(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<PurchaseReturn>()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity == null) return NotFound();
        if (entity.IsPosted) return NoContent();

        if (entity.DeductFromInventory)
            await ApplyPurchaseReturnInventoryAsync(entity, cancellationToken);

        entity.StatusKey = "completed";
        entity.StatusName = "Completed";
        entity.StatusColor = "#10b981";
        entity.IsPosted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeletePurchaseReturn(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<PurchaseReturn>()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity == null) return NotFound();
        if (entity.IsPosted) return BadRequest("Completed purchase returns cannot be deleted.");

        _context.Set<PurchaseReturn>().Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task ApplyPurchaseReturnInventoryAsync(PurchaseReturn entity, CancellationToken cancellationToken)
    {
        foreach (var line in entity.Lines.Where(x => x.ItemId != Guid.Empty))
        {
            var inventory = await _context.Set<Inventory>()
                .FirstOrDefaultAsync(x => x.ItemId == line.ItemId && x.WarehouseId == entity.WarehouseId, cancellationToken);

            if (inventory == null)
                continue;

            inventory.QuantityOnHand = Math.Max(0, inventory.QuantityOnHand - Decimal.ToInt32(line.Quantity));
            inventory.LastStockOutAt = DateTime.UtcNow;

            _context.Set<InventoryTransaction>().Add(new InventoryTransaction
            {
                InventoryId = inventory.Id,
                TransactionType = "StockOut",
                Quantity = Decimal.ToInt32(line.Quantity),
                WarehouseId = entity.WarehouseId,
                DocumentType = "PurchaseReturn",
                DocumentId = entity.Id,
                Reason = "Purchase return stock deduction",
                Notes = entity.Notes,
                CreatedByUserId = entity.CreatedByUserId
            });
        }
    }

    private async Task<string> GenerateNumberAsync(string prefix, CancellationToken cancellationToken)
    {
        var count = await _context.Set<PurchaseReturn>().CountAsync(cancellationToken) + 1;
        return $"{prefix}-{DateTime.UtcNow:yyyy}-{count:D4}";
    }

    private static void RecalculateTotals(PurchaseReturn entity)
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

    private static PurchaseReturnDto ToDto(PurchaseReturn x) => new()
    {
        Id = x.Id,
        ReturnNumber = x.ReturnNumber,
        SupplierId = x.SupplierId,
        SupplierName = x.SupplierName,
        StatusLookupId = x.StatusKey,
        StatusName = x.StatusName,
        StatusColor = x.StatusColor,
        PurchaseInvoiceId = x.PurchaseInvoiceId,
        PurchaseInvoiceNumber = x.PurchaseInvoiceNumber,
        ReturnReasonLookupId = x.ReturnReasonLookupId,
        ReturnReasonName = x.ReturnReasonName,
        ReturnDate = x.ReturnDate,
        Currency = x.Currency,
        SubTotal = x.SubTotal,
        TaxAmount = x.TaxAmount,
        Total = x.Total,
        IsPosted = x.IsPosted,
        DeductFromInventory = x.DeductFromInventory,
        Notes = x.Notes,
        WarehouseId = x.WarehouseId,
        CreatedByUserId = x.CreatedByUserId,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
        Lines = x.Lines.Select(l => new PurchaseReturnLineDto
        {
            Id = l.Id,
            PurchaseInvoiceLineId = l.PurchaseInvoiceLineId,
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

    public class CreatePurchaseReturnRequest
    {
        public Guid? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public Guid? PurchaseInvoiceId { get; set; }
        public string? PurchaseInvoiceNumber { get; set; }
        public string? ReturnReasonLookupId { get; set; }
        public string? ReturnReasonName { get; set; }
        public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
        public string? Currency { get; set; }
        public bool? DeductFromInventory { get; set; }
        public string? Notes { get; set; }
        public Guid? WarehouseId { get; set; }
        public List<CreatePurchaseReturnLineRequest> Lines { get; set; } = new();
    }

    public class CreatePurchaseReturnLineRequest
    {
        public Guid? PurchaseInvoiceLineId { get; set; }
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

    public class PurchaseReturnDto
    {
        public Guid Id { get; set; }
        public string ReturnNumber { get; set; } = string.Empty;
        public Guid? SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string StatusLookupId { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string? StatusColor { get; set; }
        public Guid? PurchaseInvoiceId { get; set; }
        public string? PurchaseInvoiceNumber { get; set; }
        public string? ReturnReasonLookupId { get; set; }
        public string? ReturnReasonName { get; set; }
        public DateTime ReturnDate { get; set; }
        public string Currency { get; set; } = "ILS";
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public bool IsPosted { get; set; }
        public bool DeductFromInventory { get; set; }
        public string? Notes { get; set; }
        public Guid? WarehouseId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<PurchaseReturnLineDto> Lines { get; set; } = new();
    }

    public class PurchaseReturnLineDto
    {
        public Guid Id { get; set; }
        public Guid? PurchaseInvoiceLineId { get; set; }
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
