using Maba.Domain.Common;

namespace Maba.Domain.Media;

public class EntityMediaLink : BaseEntity
{
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public Guid MediaAssetId { get; set; }
    public Guid MediaUsageTypeId { get; set; }
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
    
    // Navigation properties
    public MediaAsset MediaAsset { get; set; } = null!;
    public MediaUsageType MediaUsageType { get; set; } = null!;
}

