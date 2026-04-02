using Maba.Domain.Common;
using Maba.Domain.Users;

namespace Maba.Domain.SupportChat;

public class SupportConversation : BaseEntity
{
    public Guid CustomerId { get; set; }
    /// <summary>Short title shown in lists (e.g. order help, technical question).</summary>
    public string Subject { get; set; } = "Support";
    public Guid? AssignedToUserId { get; set; }
    public SupportConversationStatus Status { get; set; } = SupportConversationStatus.Open;
    public Guid? RelatedOrderId { get; set; }
    public Guid? RelatedDesignId { get; set; }

    public User Customer { get; set; } = null!;
    public User? AssignedToUser { get; set; }
    public ICollection<SupportMessage> Messages { get; set; } = new List<SupportMessage>();
}
