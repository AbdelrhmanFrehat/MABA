using Maba.Domain.Common;

namespace Maba.Domain.AiChat;

public class AiMessage : BaseEntity
{
    public Guid SessionId { get; set; }
    public Guid AiSenderTypeId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? MetaJson { get; set; }
    public int? TokensUsed { get; set; }
    public string? Model { get; set; } // e.g., "gpt-4", "gpt-3.5-turbo"
    public int? ResponseTimeMs { get; set; }
    public bool IsEdited { get; set; } = false;
    public DateTime? EditedAt { get; set; }
    
    // Navigation properties
    public AiSession Session { get; set; } = null!;
    public AiSenderType AiSenderType { get; set; } = null!;
}

