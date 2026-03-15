namespace Maba.Application.Features.Cms.DTOs;

public class PageDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleAr { get; set; }
    public string? MetaTitleEn { get; set; }
    public string? MetaTitleAr { get; set; }
    public string? MetaDescriptionEn { get; set; }
    public string? MetaDescriptionAr { get; set; }
    public bool IsHome { get; set; }
    public bool IsActive { get; set; }
    public string? TemplateKey { get; set; }
    public bool IsPublished { get; set; }
    public int Version { get; set; }
    public DateTime? PublishedAt { get; set; }
    public Guid? PublishedByUserId { get; set; }
    public string? PublishedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PageDetailDto : PageDto
{
    public List<PageSectionDraftDto> DraftSections { get; set; } = new();
    public List<PageSectionPublishedDto> PublishedSections { get; set; } = new();
}

public class PagePreviewDto : PageDto
{
    public List<PageSectionDraftDto> Sections { get; set; } = new();
}

public class PageSectionDraftDto
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }
    public Guid PageSectionTypeId { get; set; }
    public string PageSectionTypeKey { get; set; } = string.Empty;
    public Guid LayoutTypeId { get; set; }
    public string LayoutTypeKey { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleAr { get; set; }
    public string? SubtitleEn { get; set; }
    public string? SubtitleAr { get; set; }
    public string? ConfigJson { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public string? PreviewUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PageSectionPublishedDto
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }
    public Guid PageSectionTypeId { get; set; }
    public string PageSectionTypeKey { get; set; } = string.Empty;
    public Guid LayoutTypeId { get; set; }
    public string LayoutTypeKey { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleAr { get; set; }
    public string? SubtitleEn { get; set; }
    public string? SubtitleAr { get; set; }
    public string? ConfigJson { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime PublishedAt { get; set; }
    public Guid? PublishedByUserId { get; set; }
    public int Version { get; set; }
    public DateTime? UnpublishedAt { get; set; }
    public Guid? UnpublishedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

