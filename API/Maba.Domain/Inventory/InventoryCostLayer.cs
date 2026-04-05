using Maba.Domain.Catalog;
using Maba.Domain.Common;

namespace Maba.Domain.Inventory;

public class InventoryCostLayer : BaseEntity
{
    public Guid ItemId { get; set; }
    public Guid WarehouseId { get; set; }
    public int Quantity { get; set; }
    public decimal CostPerUnit { get; set; }
    public string SourceDocumentType { get; set; } = string.Empty;
    public Guid? SourceDocumentId { get; set; }

    public Item Item { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
}
