using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.ControlCenter.Instances.DTOs;
using Maba.Application.Features.ControlCenter.Instances.Commands;
using Maba.Domain.ControlCenter;

namespace Maba.Application.Features.ControlCenter.Instances.Handlers;

public class RegisterInstanceCommandHandler : IRequestHandler<RegisterInstanceCommand, ControlCenterInstanceDto>
{
    private readonly IApplicationDbContext _db;

    public RegisterInstanceCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ControlCenterInstanceDto> Handle(RegisterInstanceCommand request, CancellationToken cancellationToken)
    {
        // For now, do not deduplicate by hostname; just create a new instance per registration
        var entity = new ControlCenterInstance
        {
            OrgId = request.OrgId,
            SiteId = request.SiteId,
            MachineId = request.MachineId,
            Hostname = request.Hostname,
            OsInfo = request.OsInfo,
            CoreVersion = request.CoreVersion,
            InstalledModulesJson = request.InstalledModulesJson,
            LastSeenAt = DateTime.UtcNow
        };

        _db.Set<ControlCenterInstance>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        // Reload to ensure any defaults are materialized
        var persisted = await _db.Set<ControlCenterInstance>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == entity.Id, cancellationToken);

        return ControlCenterInstanceDto.FromEntity(persisted);
    }
}

