using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cnc.DTOs;
using Maba.Domain.Cnc;

namespace Maba.Application.Features.Cnc.Materials.Queries;

public class GetCncMaterialByIdQueryHandler : IRequestHandler<GetCncMaterialByIdQuery, CncMaterialDto?>
{
    private readonly IApplicationDbContext _context;

    public GetCncMaterialByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CncMaterialDto?> Handle(GetCncMaterialByIdQuery request, CancellationToken cancellationToken)
    {
        var material = await _context.Set<CncMaterial>()
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        return material == null ? null : CncMaterialDto.FromEntity(material);
    }
}
