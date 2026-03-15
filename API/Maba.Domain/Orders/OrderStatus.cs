using Maba.Domain.Common;

namespace Maba.Domain.Orders;

public class OrderStatus : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

