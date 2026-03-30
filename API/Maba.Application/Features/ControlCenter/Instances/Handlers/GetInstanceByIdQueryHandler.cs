using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.ControlCenter.Instances.DTOs;
using Maba.Application.Features.ControlCenter.Instances.Queries;
using Maba.Domain.ControlCenter;

namespace Maba.Application.Features.ControlCenter.Instances.Handlers;

public class GetInstanceByIdQueryHandler : IRequestHandler<GetInstanceByIdQuery, ControlCenterInstanceDto?>
{
    private readonly IApplicationDbContext _db;

    public GetInstanceByIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ControlCenterInstanceDto?> Handle(GetInstanceByIdQuery request, CancellationToken cancellationToken)
    {
        var instance = await _db.Set<ControlCenterInstance>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.OrgId == request.OrgId, cancellationToken);

        return instance == null ? null : ControlCenterInstanceDto.FromEntity(instance);
    }
}

