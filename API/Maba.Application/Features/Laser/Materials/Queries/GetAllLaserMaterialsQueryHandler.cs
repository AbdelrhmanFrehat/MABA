using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Laser.DTOs;
using Maba.Domain.Laser;

namespace Maba.Application.Features.Laser.Materials.Queries;

public class GetAllLaserMaterialsQueryHandler : IRequestHandler<GetAllLaserMaterialsQuery, List<LaserMaterialDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllLaserMaterialsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LaserMaterialDto>> Handle(GetAllLaserMaterialsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<LaserMaterial>().AsQueryable();

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
            query = query.Where(m => m.Type == request.Type || m.Type == "both");
        }

        var materials = await query
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.NameEn)
            .ToListAsync(cancellationToken);

        return materials.Select(m => new LaserMaterialDto
        {
            Id = m.Id,
            NameEn = m.NameEn,
            NameAr = m.NameAr,
            Type = m.Type,
            MinThicknessMm = m.MinThicknessMm,
            MaxThicknessMm = m.MaxThicknessMm,
            NotesEn = m.NotesEn,
            NotesAr = m.NotesAr,
            IsMetal = m.IsMetal,
            IsActive = m.IsActive,
            SortOrder = m.SortOrder,
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt
        }).ToList();
    }
}
