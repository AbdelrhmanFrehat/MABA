using Maba.Domain.Common;

namespace Maba.Domain.Cms;

public class PageSectionType : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<PageSectionDraft> DraftSections { get; set; } = new List<PageSectionDraft>();
    public ICollection<PageSectionPublished> PublishedSections { get; set; } = new List<PageSectionPublished>();
}

