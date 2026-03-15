using Maba.Domain.Common;

namespace Maba.Domain.Catalog;

public class Tag : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? Color { get; set; } // Hex color code
    public string? Icon { get; set; } // Icon name or URL
    public int UsageCount { get; set; } = 0; // Computed or maintained
    
    // Navigation properties
    public ICollection<ItemTag> ItemTags { get; set; } = new List<ItemTag>();
}

