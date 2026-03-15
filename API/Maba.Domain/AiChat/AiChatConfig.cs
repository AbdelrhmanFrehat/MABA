using Maba.Domain.Common;

namespace Maba.Domain.AiChat;

public class AiChatConfig : BaseEntity
{
    public string Key { get; set; } = string.Empty; // e.g., "DefaultModel", "MaxTokens", "Temperature"
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

