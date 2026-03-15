using Maba.Domain.Common;
using Maba.Domain.Users;

namespace Maba.Domain.Catalog;

public class Comment : BaseEntity
{
    public Guid ItemId { get; set; }
    public Guid UserId { get; set; }
    public string Body { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
    public bool IsApproved { get; set; }
    
    // Navigation properties
    public Item Item { get; set; } = null!;
    public User User { get; set; } = null!;
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}

