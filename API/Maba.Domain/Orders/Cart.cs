using Maba.Domain.Common;
using Maba.Domain.Users;

namespace Maba.Domain.Orders;

public class Cart : BaseEntity
{
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; }
    public string? CouponCode { get; set; }
    public decimal CouponDiscount { get; set; }
    public DateTime? ExpiresAt { get; set; }
    
    // Navigation properties
    public User? User { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
