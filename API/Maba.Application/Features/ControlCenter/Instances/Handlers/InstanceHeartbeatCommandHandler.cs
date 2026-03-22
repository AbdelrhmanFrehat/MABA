using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.ControlCenter.Instances.Commands;
using Maba.Application.Features.ControlCenter.Instances.DTOs;
using Maba.Domain.ControlCenter;

namespace Maba.Application.Features.ControlCenter.Instances.Handlers;

public class InstanceHeartbeatCommandHandler : IRequestHandler<InstanceHeartbeatCommand, ControlCenterInstanceDto>
{
    private readonly IApplicationDbContext _db;

    public InstanceHeartbeatCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ControlCenterInstanceDto> Handle(InstanceHeartbeatCommand request, CancellationToken cancellationToken)
    {
        var instance = await _db.Set<ControlCenterInstance>()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.OrgId == request.OrgId, cancellationToken);

        if (instance == null)
        {
            throw new KeyNotFoundException($"ControlCenterInstance {request.Id} was not found for this org.");
        }

        instance.CoreVersion = request.CoreVersion;
        instance.OsInfo = request.OsInfo ?? instance.OsInfo;
        instance.InstalledModulesJson = request.InstalledModulesJson ?? instance.InstalledModulesJson;
        instance.LastSeenAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return ControlCenterInstanceDto.FromEntity(instance);
    }
}

