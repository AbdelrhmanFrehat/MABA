using Maba.Domain.Common;

namespace Maba.Domain.Catalog;

public class Inventory : BaseEntity
{
    public Guid ItemId { get; set; }
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; } = 0;
    public int QuantityOnOrder { get; set; } = 0;
    public int ReorderLevel { get; set; }
    public decimal? CostPerUnit { get; set; }
    public Guid? WarehouseId { get; set; } // For multi-warehouse support
    public DateTime? LastStockInAt { get; set; }
    public DateTime? LastStockOutAt { get; set; }
    
    // Computed property (not stored in DB)
    public int QuantityAvailable => QuantityOnHand - QuantityReserved;
    
    // Navigation properties
    public Item Item { get; set; } = null!;
    public ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
}

