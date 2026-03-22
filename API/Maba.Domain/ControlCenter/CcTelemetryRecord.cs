using Maba.Domain.Common;

namespace Maba.Domain.ControlCenter;

public class CcTelemetryRecord : BaseEntity
{
    public Guid OrgId { get; set; }
    public Guid? SiteId { get; set; }

    public DateTime Timestamp { get; set; }

    public Guid? InstanceId { get; set; }
    public Guid? DeviceId { get; set; }

    public string MetricType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? TagsJson { get; set; }
}

