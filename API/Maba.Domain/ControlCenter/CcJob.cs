using Maba.Domain.Common;

namespace Maba.Domain.ControlCenter;

public enum CcJobStatus
{
    Pending,
    Ready,
    InProgress,
    Completed,
    Failed,
    Cancelled,
    Queued = Pending,
    Running = InProgress,
    Paused = Ready,
    Canceled = Cancelled
}

public class CcJob : BaseEntity
{
    public Guid? OrgId { get; set; }
    public Guid? SiteId { get; set; }

    public Guid? TemplateId { get; set; }
    public Guid? DeviceId { get; set; }

    public string JobReference { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public Guid? SourceId { get; set; }
    public string? SourceReference { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CustomerName { get; set; }
    public string? MachineType { get; set; }
    public Guid? ModuleId { get; set; }
    public string? Priority { get; set; }

    public CcJobStatus Status { get; set; } = CcJobStatus.Pending;
    public double? Progress { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public string? ResultSummary { get; set; }
    public string? AttachmentsJson { get; set; }
    public string? PayloadJson { get; set; }

    /// <summary>
    /// Arbitrary parameters payload provided at job creation.
    /// </summary>
    public string? ParametersJson { get; set; }
}

