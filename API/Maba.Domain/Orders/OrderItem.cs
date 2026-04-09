using Maba.Domain.Common;
using Maba.Domain.Catalog;

namespace Maba.Domain.Orders;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid? ItemId { get; set; }
    /// <summary>Free-text description for service lines (no catalog item).</summary>
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public string? MetaJson { get; set; }
    
    // Navigation properties
    public Order Order { get; set; } = null!;
    public Item? Item { get; set; }
}

