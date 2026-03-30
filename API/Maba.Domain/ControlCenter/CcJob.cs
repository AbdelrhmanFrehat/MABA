using Maba.Domain.Common;

namespace Maba.Domain.ControlCenter;

public enum CcJobStatus
{
    Queued,
    Running,
    Paused,
    Completed,
    Failed,
    Canceled
}

public class CcJob : BaseEntity
{
    public Guid OrgId { get; set; }
    public Guid SiteId { get; set; }

    public Guid TemplateId { get; set; }
    public Guid DeviceId { get; set; }

    public CcJobStatus Status { get; set; } = CcJobStatus.Queued;
    public double? Progress { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public string? ResultSummary { get; set; }

    /// <summary>
    /// Arbitrary parameters payload provided at job creation.
    /// </summary>
    public string? ParametersJson { get; set; }
}

