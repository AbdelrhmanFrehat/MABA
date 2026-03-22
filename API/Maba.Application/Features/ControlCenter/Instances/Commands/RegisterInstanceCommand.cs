using MediatR;
using Maba.Application.Features.ControlCenter.Instances.DTOs;

namespace Maba.Application.Features.ControlCenter.Instances.Commands;

public class RegisterInstanceCommand : IRequest<ControlCenterInstanceDto>
{
    public Guid OrgId { get; set; }
    public Guid? SiteId { get; set; }
    public Guid? MachineId { get; set; }
    public string Hostname { get; set; } = string.Empty;
    public string? OsInfo { get; set; }
    public string CoreVersion { get; set; } = string.Empty;
    public string? InstalledModulesJson { get; set; }
}

