using Maba.Domain.Common;

namespace Maba.Domain.Projects;

public enum ProjectCategory
{
    Electronics,
    Mechanical,
    Software,
    Automation,
    Custom,
    Robotics,
    CNC,
    Monitoring,
    Embedded,
    RnD
}

public enum ProjectStatus
{
    Draft,
    InProgress,
    Completed,
    Published,
    Archived,
    Delivered,
    Prototype,
    Concept
}

public class Project : BaseEntity
{
    public string TitleEn { get; set; } = string.Empty;
    public string? TitleAr { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? SummaryEn { get; set; }
    public string? SummaryAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? ImageUrl { get; set; }
    public ProjectCategory Category { get; set; } = ProjectCategory.Custom;
    public ProjectStatus Status { get; set; } = ProjectStatus.Published;
    public int Year { get; set; }
    public string? TechStackJson { get; set; }
    public string? HighlightsJson { get; set; }
    public string? GalleryJson { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; }
}
