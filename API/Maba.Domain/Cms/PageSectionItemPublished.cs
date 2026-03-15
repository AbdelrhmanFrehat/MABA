using Maba.Domain.Common;

namespace Maba.Domain.Cms;

public class PageSectionItemPublished : BaseEntity
{
    public Guid PageSectionPublishedId { get; set; }
    public string LinkedEntityType { get; set; } = string.Empty;
    public Guid LinkedEntityId { get; set; }
    public string? ExtraConfigJson { get; set; }
    public int SortOrder { get; set; }
    
    // Navigation properties
    public PageSectionPublished PageSectionPublished { get; set; } = null!;
}

