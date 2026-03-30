using Maba.Domain.Common;

namespace Maba.Domain.ControlCenter;

public class CcAuditEvent : BaseEntity
{
    public DateTime Timestamp { get; set; }

    public Guid UserId { get; set; }
    public Guid OrgId { get; set; }
    public Guid? SiteId { get; set; }

    public Guid? InstanceId { get; set; }
    public Guid? DeviceId { get; set; }

    public string Action { get; set; } = string.Empty;
    public string? MetadataJson { get; set; }
}

