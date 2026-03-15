using Maba.Domain.Common;

namespace Maba.Domain.Media;

public class MediaUsageType : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<EntityMediaLink> EntityMediaLinks { get; set; } = new List<EntityMediaLink>();
}

