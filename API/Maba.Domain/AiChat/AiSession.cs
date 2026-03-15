using Maba.Domain.Common;
using Maba.Domain.Users;

namespace Maba.Domain.AiChat;

public class AiSession : BaseEntity
{
    public Guid? UserId { get; set; }
    public Guid? AiSessionSourceId { get; set; }
    public string? Title { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public User? User { get; set; }
    public AiSessionSource? AiSessionSource { get; set; }
    public ICollection<AiMessage> Messages { get; set; } = new List<AiMessage>();
}

