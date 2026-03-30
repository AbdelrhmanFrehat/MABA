using Maba.Domain.Common;

namespace Maba.Domain.ControlCenter;

public class ControlCenterInstance : BaseEntity
{
    public Guid OrgId { get; set; }
    public Guid? SiteId { get; set; }
    public Guid? MachineId { get; set; }

    public string Hostname { get; set; } = string.Empty;
    public string? OsInfo { get; set; }
    public string CoreVersion { get; set; } = string.Empty;

    public DateTime? LastSeenAt { get; set; }

    /// <summary>
    /// JSON payload describing installed modules and versions.
    /// </summary>
    public string? InstalledModulesJson { get; set; }
}

