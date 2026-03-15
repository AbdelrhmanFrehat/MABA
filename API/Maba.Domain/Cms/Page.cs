using Maba.Domain.Common;
using Maba.Domain.Users;

namespace Maba.Domain.Cms;

public class Page : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleAr { get; set; }
    public string? MetaTitleEn { get; set; }
    public string? MetaTitleAr { get; set; }
    public string? MetaDescriptionEn { get; set; }
    public string? MetaDescriptionAr { get; set; }
    public bool IsHome { get; set; }
    public bool IsActive { get; set; } = true;
    public string? TemplateKey { get; set; }
    public bool IsPublished { get; set; } = false;
    public int Version { get; set; } = 1;
    public DateTime? PublishedAt { get; set; }
    public Guid? PublishedByUserId { get; set; }
    
    // Navigation properties
    public User? PublishedByUser { get; set; }
    public ICollection<PageSectionDraft> DraftSections { get; set; } = new List<PageSectionDraft>();
    public ICollection<PageSectionPublished> PublishedSections { get; set; } = new List<PageSectionPublished>();
    public ICollection<PageVersion> Versions { get; set; } = new List<PageVersion>();
}

