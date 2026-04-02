using Maba.Domain.Common;
using Maba.Domain.Users;

namespace Maba.Domain.Catalog;

public class Review : BaseEntity
{
    public Guid ItemId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public bool IsApproved { get; set; }
    /// <summary>Moderation: rejected by admin (not shown publicly).</summary>
    public bool IsRejected { get; set; }
    
    // Navigation properties
    public Item Item { get; set; } = null!;
    public User User { get; set; } = null!;
}

