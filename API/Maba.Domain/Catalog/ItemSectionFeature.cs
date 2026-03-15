using Maba.Domain.Common;

namespace Maba.Domain.Catalog;

public class ItemSectionFeature : BaseEntity
{
    public Guid ItemSectionId { get; set; }
    public string? TextEn { get; set; }
    public string? TextAr { get; set; }
    public int SortOrder { get; set; }
    
    // Navigation properties
    public ItemSection ItemSection { get; set; } = null!;
}

