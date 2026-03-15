using Maba.Domain.Common;

namespace Maba.Domain.Projects;

public enum ProjectRequestType
{
    SimilarToExisting,
    Custom
}

public enum ProjectRequestStatus
{
    New,
    InReview,
    Quoted,
    InProgress,
    Closed
}

public class ProjectRequest : BaseEntity
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    
    public ProjectRequestType RequestType { get; set; } = ProjectRequestType.Custom;
    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }
    
    public ProjectCategory? Category { get; set; }
    public string? BudgetRange { get; set; }
    public string? Timeline { get; set; }
    public string? Description { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentFileName { get; set; }
    
    public ProjectRequestStatus Status { get; set; } = ProjectRequestStatus.New;
    public string? AdminNotes { get; set; }
}
