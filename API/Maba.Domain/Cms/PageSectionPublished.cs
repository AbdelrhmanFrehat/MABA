using Maba.Domain.Common;
using Maba.Domain.Users;

namespace Maba.Domain.Cms;

public class PageSectionPublished : BaseEntity
{
    public Guid PageId { get; set; }
    public Guid PageSectionTypeId { get; set; }
    public Guid LayoutTypeId { get; set; }
    public string? TitleEn { get; set; }
    public string? TitleAr { get; set; }
    public string? SubtitleEn { get; set; }
    public string? SubtitleAr { get; set; }
    public string? ConfigJson { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime PublishedAt { get; set; }
    public Guid? PublishedByUserId { get; set; }
    public int Version { get; set; } = 1;
    public DateTime? UnpublishedAt { get; set; }
    public Guid? UnpublishedByUserId { get; set; }
    
    // Navigation properties
    public Page Page { get; set; } = null!;
    public PageSectionType PageSectionType { get; set; } = null!;
    public LayoutType LayoutType { get; set; } = null!;
    public User? PublishedByUser { get; set; }
    public User? UnpublishedByUser { get; set; }
    public ICollection<PageSectionItemPublished> Items { get; set; } = new List<PageSectionItemPublished>();
}

