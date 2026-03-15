using Maba.Domain.Common;

namespace Maba.Domain.Cms;

public class PageSectionItemDraft : BaseEntity
{
    public Guid PageSectionDraftId { get; set; }
    public string LinkedEntityType { get; set; } = string.Empty;
    public Guid LinkedEntityId { get; set; }
    public string? ExtraConfigJson { get; set; }
    public int SortOrder { get; set; }
    
    // Navigation properties
    public PageSectionDraft PageSectionDraft { get; set; } = null!;
}

