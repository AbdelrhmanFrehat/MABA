using Maba.Domain.Common;

namespace Maba.Domain.Cms;

public class PageTemplate : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? TemplateJson { get; set; } // JSON structure defining the template
    public string? PreviewImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<Page> Pages { get; set; } = new List<Page>();
}

