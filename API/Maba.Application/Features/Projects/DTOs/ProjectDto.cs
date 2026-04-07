using Maba.Domain.Projects;

namespace Maba.Application.Features.Projects.DTOs;

public class ProjectDto
{
    public Guid Id { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string? TitleAr { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? SummaryEn { get; set; }
    public string? SummaryAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? CoverImageUrl { get; set; }
    public ProjectCategory Category { get; set; }
    public string CategoryName => Category.ToString();
    public ProjectStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public int Year { get; set; }
    public List<string> TechStack { get; set; } = new();
    public List<string> Highlights { get; set; } = new();
    public List<string> Gallery { get; set; } = new();
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ProjectListDto
{
    public Guid Id { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string? TitleAr { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? SummaryEn { get; set; }
    public string? SummaryAr { get; set; }
    public string? CoverImageUrl { get; set; }
    public ProjectCategory Category { get; set; }
    public string CategoryName => Category.ToString();
    public ProjectStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public int Year { get; set; }
    public List<string> TechStack { get; set; } = new();
    public bool IsFeatured { get; set; }
}

public class ProjectRequestDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public ProjectRequestType RequestType { get; set; }
    public string RequestTypeName => RequestType.ToString();
    public Guid? ProjectId { get; set; }
    public string? ProjectTitle { get; set; }
    public string? ProjectType { get; set; }
    public string? MainDomain { get; set; }
    public List<string> RequiredCapabilities { get; set; } = new();
    public ProjectCategory? Category { get; set; }
    public string? CategoryName => Category?.ToString();
    public string? BudgetRange { get; set; }
    public string? Timeline { get; set; }
    public string? ProjectStage { get; set; }
    public string? Description { get; set; }
    public string? ProjectDescription { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentFileName { get; set; }
    public List<ProjectRequestAttachmentDto> Attachments { get; set; } = new();
    public ProjectRequestStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string WorkflowStatus { get; set; } = "New";
    public string? AdminNotes { get; set; }
    // Assignment
    public string? AssignedToUserId { get; set; }
    public string? AssignedToName { get; set; }
    // Internal engineering fields
    public string? Priority { get; set; }
    public string? TechnicalFeasibility { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string? EstimatedTimeline { get; set; }
    public string? ComplexityLevel { get; set; }
    public string? InternalNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ProjectRequestActivityDto
{
    public Guid Id { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateProjectRequest
{
    public string TitleEn { get; set; } = string.Empty;
    public string? TitleAr { get; set; }
    public string? Slug { get; set; }
    public string? SummaryEn { get; set; }
    public string? SummaryAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? CoverImageUrl { get; set; }
    public ProjectCategory Category { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
    public int Year { get; set; }
    public List<string>? TechStack { get; set; }
    public List<string>? Highlights { get; set; }
    public List<string>? Gallery { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

public class UpdateProjectRequest : CreateProjectRequest
{
    public Guid Id { get; set; }
}

public class CreateProjectRequestRfq
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public ProjectRequestType RequestType { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectType { get; set; }
    public string? MainDomain { get; set; }
    public List<string>? RequiredCapabilities { get; set; }
    public ProjectCategory? Category { get; set; }
    public string? BudgetRange { get; set; }
    public string? Timeline { get; set; }
    public string? ProjectStage { get; set; }
    public string? Description { get; set; }
    public string? ProjectDescription { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentFileName { get; set; }
    public List<ProjectRequestAttachmentDto>? Attachments { get; set; }
}

public class UpdateProjectRequestRfq
{
    public Guid Id { get; set; }
    public ProjectRequestStatus? Status { get; set; }
    public string? WorkflowStatus { get; set; }
    public string? AdminNotes { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public ProjectRequestType? RequestType { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectType { get; set; }
    public string? MainDomain { get; set; }
    public List<string>? RequiredCapabilities { get; set; }
    public ProjectCategory? Category { get; set; }
    public string? BudgetRange { get; set; }
    public string? Timeline { get; set; }
    public string? ProjectStage { get; set; }
    public string? Description { get; set; }
    public string? ProjectDescription { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentFileName { get; set; }
    public List<ProjectRequestAttachmentDto>? Attachments { get; set; }
    // Assignment
    public string? AssignedToUserId { get; set; }
    public string? AssignedToName { get; set; }
    // Internal engineering fields
    public string? Priority { get; set; }
    public string? TechnicalFeasibility { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string? EstimatedTimeline { get; set; }
    public string? ComplexityLevel { get; set; }
    public string? InternalNotes { get; set; }
    public string? UpdatedBy { get; set; }
}

public class ProjectsListResponse
{
    public List<ProjectListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
