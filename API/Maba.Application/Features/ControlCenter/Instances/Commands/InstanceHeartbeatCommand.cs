using MediatR;
using Maba.Application.Features.ControlCenter.Instances.DTOs;

namespace Maba.Application.Features.ControlCenter.Instances.Commands;

public class InstanceHeartbeatCommand : IRequest<ControlCenterInstanceDto>
{
    public Guid Id { get; set; }
    public Guid OrgId { get; set; }
    public string CoreVersion { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? OsInfo { get; set; }
    public string? InstalledModulesJson { get; set; }
}

