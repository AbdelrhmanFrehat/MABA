using Maba.Domain.Common;

namespace Maba.Domain.Catalog;

public class InventoryTransaction : BaseEntity
{
    public Guid InventoryId { get; set; }
    public string TransactionType { get; set; } = string.Empty; // StockIn, StockOut, Adjustment, Reservation, Release
    public int Quantity { get; set; }
    public decimal? CostPerUnit { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? DocumentType { get; set; }
    public Guid? DocumentId { get; set; }
    public Guid? UnitOfMeasureId { get; set; }
    public int? BaseQuantity { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public Guid? OrderId { get; set; } // If related to an order
    public Guid? CreatedByUserId { get; set; }
    
    // Navigation properties
    public Inventory Inventory { get; set; } = null!;
    public UnitOfMeasure? UnitOfMeasure { get; set; }
}

