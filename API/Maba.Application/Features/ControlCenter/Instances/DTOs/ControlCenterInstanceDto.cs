using Maba.Domain.ControlCenter;

namespace Maba.Application.Features.ControlCenter.Instances.DTOs;

public class ControlCenterInstanceDto
{
    public Guid Id { get; set; }
    public Guid OrgId { get; set; }
    public Guid? SiteId { get; set; }
    public Guid? MachineId { get; set; }
    public string Hostname { get; set; } = string.Empty;
    public string? OsInfo { get; set; }
    public string CoreVersion { get; set; } = string.Empty;
    public DateTime? LastSeenAt { get; set; }
    public string? InstalledModulesJson { get; set; }

    public static ControlCenterInstanceDto FromEntity(ControlCenterInstance entity) =>
        new()
        {
            Id = entity.Id,
            OrgId = entity.OrgId,
            SiteId = entity.SiteId,
            MachineId = entity.MachineId,
            Hostname = entity.Hostname,
            OsInfo = entity.OsInfo,
            CoreVersion = entity.CoreVersion,
            LastSeenAt = entity.LastSeenAt,
            InstalledModulesJson = entity.InstalledModulesJson
        };
}

