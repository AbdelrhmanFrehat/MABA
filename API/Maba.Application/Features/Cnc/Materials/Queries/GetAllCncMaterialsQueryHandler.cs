using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cnc.DTOs;
using Maba.Domain.Cnc;

namespace Maba.Application.Features.Cnc.Materials.Queries;

public class GetAllCncMaterialsQueryHandler : IRequestHandler<GetAllCncMaterialsQuery, List<CncMaterialDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllCncMaterialsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CncMaterialDto>> Handle(GetAllCncMaterialsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<CncMaterial>().AsQueryable();

        if (request.ActiveOnly == true)
        {
            query = query.Where(m => m.IsActive);
        }

        if (request.ExcludeMetal)
        {
            query = query.Where(m => !m.IsMetal);
        }

        if (!string.IsNullOrEmpty(request.Type))
        {
            var t = request.Type.Trim().ToLowerInvariant();
            query = t switch
            {
                "routing" => query.Where(m => m.Type == "routing" || m.Type == "both"),
                "pcb" => query.Where(m => m.Type == "pcb" || m.Type == "both"),
                _ => query.Where(m => m.Type == t)
            };
        }

        if (request.IsPcbOnly.HasValue)
        {
            query = query.Where(m => m.IsPcbOnly == request.IsPcbOnly.Value);
        }

        var materials = await query
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.NameEn)
            .ToListAsync(cancellationToken);

        return materials.Select(CncMaterialDto.FromEntity).ToList();
    }
}
