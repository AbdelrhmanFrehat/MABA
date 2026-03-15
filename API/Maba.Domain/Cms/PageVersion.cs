using Maba.Domain.Common;
using Maba.Domain.Users;

namespace Maba.Domain.Cms;

public class PageVersion : BaseEntity
{
    public Guid PageId { get; set; }
    public int VersionNumber { get; set; }
    public string? TitleEn { get; set; }
    public string? TitleAr { get; set; }
    public string? MetaTitleEn { get; set; }
    public string? MetaTitleAr { get; set; }
    public string? MetaDescriptionEn { get; set; }
    public string? MetaDescriptionAr { get; set; }
    public string? ContentJson { get; set; } // Snapshot of page content at this version
    public Guid? CreatedByUserId { get; set; }
    public DateTime? RestoredAt { get; set; }
    public Guid? RestoredByUserId { get; set; }
    
    // Navigation properties
    public Page Page { get; set; } = null!;
    public User? CreatedByUser { get; set; }
    public User? RestoredByUser { get; set; }
}

