using Maba.Domain.Common;
using Maba.Domain.Users;

namespace Maba.Domain.Wishlist;

public class Wishlist : BaseEntity
{
    public Guid UserId { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
}
