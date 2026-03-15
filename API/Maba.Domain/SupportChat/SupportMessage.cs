using Maba.Domain.Common;
using Maba.Domain.Users;

namespace Maba.Domain.SupportChat;

public class SupportMessage : BaseEntity
{
    public Guid ConversationId { get; set; }
    public Guid SenderUserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public string? AttachmentFileName { get; set; }
    public DateTime? ReadAt { get; set; }

    public SupportConversation Conversation { get; set; } = null!;
    public User SenderUser { get; set; } = null!;
}
