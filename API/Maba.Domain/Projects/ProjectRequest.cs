using Maba.Domain.Common;
using Maba.Domain.Crm;
using Maba.Domain.Users;

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

// Engineering workflow statuses (string-based for extensibility)
public static class ProjectRequestWorkflowStatus
{
    public const string New = "New";
    public const string UnderReview = "UnderReview";
    public const string WaitingForInfo = "WaitingForInfo";
    public const string TechnicalReview = "TechnicalReview";
    public const string QuotationPreparation = "QuotationPreparation";
    public const string QuoteSent = "QuoteSent";
    public const string Approved = "Approved";
    public const string InExecution = "InExecution";
    public const string Completed = "Completed";
    public const string Rejected = "Rejected";

    public static readonly IReadOnlyList<string> All = new[]
    {
        New, UnderReview, WaitingForInfo, TechnicalReview,
        QuotationPreparation, QuoteSent, Approved, InExecution, Completed, Rejected
    };
}

public static class ProjectRequestPriority
{
    public const string Low = "Low";
    public const string Medium = "Medium";
    public const string High = "High";
    public const string Critical = "Critical";
}

public static class ProjectRequestFeasibility
{
    public const string Feasible = "Feasible";
    public const string PartiallyFeasible = "PartiallyFeasible";
    public const string NotFeasible = "NotFeasible";
}

public static class ProjectRequestComplexity
{
    public const string Simple = "Simple";
    public const string Moderate = "Moderate";
    public const string Complex = "Complex";
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

    public string? ProjectType { get; set; }
    public string? MainDomain { get; set; }
    public string? RequiredCapabilitiesJson { get; set; }
    public ProjectCategory? Category { get; set; }
    public string? BudgetRange { get; set; }
    public string? Timeline { get; set; }
    public string? ProjectStage { get; set; }
    public string? Description { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentFileName { get; set; }
    public string? AttachmentsJson { get; set; }

    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    // Legacy status (preserved for backward compat)
    public ProjectRequestStatus Status { get; set; } = ProjectRequestStatus.New;

    // Engineering workflow status (new primary status)
    public string WorkflowStatus { get; set; } = ProjectRequestWorkflowStatus.New;

    public string? AdminNotes { get; set; }

    // Assignment
    public string? AssignedToUserId { get; set; }
    public string? AssignedToName { get; set; }

    // Internal engineering fields (admin only)
    public string? Priority { get; set; }
    public string? TechnicalFeasibility { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string? EstimatedTimeline { get; set; }
    public string? ComplexityLevel { get; set; }
    public string? InternalNotes { get; set; }

    public ICollection<ProjectRequestActivity> Activities { get; set; } = new List<ProjectRequestActivity>();
}

public class ProjectRequestActivity : BaseEntity
{
    public Guid ProjectRequestId { get; set; }
    public ProjectRequest ProjectRequest { get; set; } = null!;
    public string ActionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CreatedBy { get; set; }
}
