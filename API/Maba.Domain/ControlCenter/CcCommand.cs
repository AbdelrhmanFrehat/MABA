using Maba.Domain.Common;

namespace Maba.Domain.ControlCenter;

public enum CcCommandTargetType
{
    Instance,
    Device
}

public enum CcCommandStatus
{
    Queued,
    Acknowledged,
    InProgress,
    Completed,
    Failed
}

public class CcCommand : BaseEntity
{
    public Guid OrgId { get; set; }
    public Guid SiteId { get; set; }

    public CcCommandTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }

    public string CommandType { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;

    public CcCommandStatus Status { get; set; } = CcCommandStatus.Queued;

    public DateTime QueuedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public string? ResultPayloadJson { get; set; }
    public string? ErrorMessage { get; set; }
}

