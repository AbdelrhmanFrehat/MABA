using Maba.Domain.Common;
using Maba.Domain.Users;

namespace Maba.Domain.Cms;

public class PageSectionDraft : BaseEntity
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
    public Guid? CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public string? PreviewUrl { get; set; }
    
    // Navigation properties
    public Page Page { get; set; } = null!;
    public PageSectionType PageSectionType { get; set; } = null!;
    public LayoutType LayoutType { get; set; } = null!;
    public User? CreatedByUser { get; set; }
    public User? UpdatedByUser { get; set; }
    public ICollection<PageSectionItemDraft> Items { get; set; } = new List<PageSectionItemDraft>();
}

