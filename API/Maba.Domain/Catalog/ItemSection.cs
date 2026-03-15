using Maba.Domain.Common;

namespace Maba.Domain.Catalog;

public class ItemSection : BaseEntity
{
    public Guid ItemId { get; set; }
    public string? TitleEn { get; set; }
    public string? TitleAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public int SortOrder { get; set; }
    
    // Navigation properties
    public Item Item { get; set; } = null!;
    public ICollection<ItemSectionFeature> Features { get; set; } = new List<ItemSectionFeature>();
}

