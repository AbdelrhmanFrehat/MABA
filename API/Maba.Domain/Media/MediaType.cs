using Maba.Domain.Common;

namespace Maba.Domain.Media;

public class MediaType : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<MediaAsset> MediaAssets { get; set; } = new List<MediaAsset>();
}

