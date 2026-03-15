using Maba.Domain.Common;

namespace Maba.Domain.AiChat;

public class AiSenderType : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<AiMessage> Messages { get; set; } = new List<AiMessage>();
}

