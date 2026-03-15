using Maba.Domain.Common;
using Maba.Domain.Users;

namespace Maba.Domain.SupportChat;

public class SupportConversation : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public SupportConversationStatus Status { get; set; } = SupportConversationStatus.Open;

    public User Customer { get; set; } = null!;
    public User? AssignedToUser { get; set; }
    public ICollection<SupportMessage> Messages { get; set; } = new List<SupportMessage>();
}
