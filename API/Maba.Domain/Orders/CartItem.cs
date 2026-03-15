using Maba.Domain.Common;
using Maba.Domain.Catalog;

namespace Maba.Domain.Orders;

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Guid ItemId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    // Navigation properties
    public Cart Cart { get; set; } = null!;
    public Item Item { get; set; } = null!;
}
