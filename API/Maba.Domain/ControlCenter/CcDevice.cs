using Maba.Domain.Common;

namespace Maba.Domain.ControlCenter;

public class CcDevice : BaseEntity
{
    public Guid OrgId { get; set; }
    public Guid SiteId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // e.g. dexter-arm, maba-scara

    public string Status { get; set; } = "unknown";
    public string? FirmwareVersion { get; set; }
    public Guid? ModuleId { get; set; }

    public string? Location { get; set; }
    public string? TagsJson { get; set; }
    public string? MetadataJson { get; set; }
}

