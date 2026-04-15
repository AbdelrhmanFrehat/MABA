using Maba.Domain.Catalog;
using Maba.Domain.Common;

namespace Maba.Domain.Wishlist;

public class WishlistItem : BaseEntity
{
    public Guid WishlistId { get; set; }
    public Guid ItemId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Wishlist Wishlist { get; set; } = null!;
    public Item Item { get; set; } = null!;
}
