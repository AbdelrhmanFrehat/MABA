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
            query = query.Where(m => m.Type == request.Type);
        }

        if (request.IsPcbOnly.HasValue)
        {
            query = query.Where(m => m.IsPcbOnly == request.IsPcbOnly.Value);
        }

        var materials = await query
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.NameEn)
            .ToListAsync(cancellationToken);

        return materials.Select(MapToDto).ToList();
    }

    private static CncMaterialDto MapToDto(CncMaterial m) => new()
    {
        Id = m.Id,
        NameEn = m.NameEn,
        NameAr = m.NameAr,
        DescriptionEn = m.DescriptionEn,
        DescriptionAr = m.DescriptionAr,
        Type = m.Type,
        MinThicknessMm = m.MinThicknessMm,
        MaxThicknessMm = m.MaxThicknessMm,
        IsMetal = m.IsMetal,
        IsActive = m.IsActive,
        SortOrder = m.SortOrder,
        AllowCut = m.AllowCut,
        AllowEngrave = m.AllowEngrave,
        AllowPocket = m.AllowPocket,
        AllowDrill = m.AllowDrill,
        MaxCutDepthMm = m.MaxCutDepthMm,
        MaxEngraveDepthMm = m.MaxEngraveDepthMm,
        MaxPocketDepthMm = m.MaxPocketDepthMm,
        MaxDrillDepthMm = m.MaxDrillDepthMm,
        CutNotesEn = m.CutNotesEn,
        CutNotesAr = m.CutNotesAr,
        EngraveNotesEn = m.EngraveNotesEn,
        EngraveNotesAr = m.EngraveNotesAr,
        PocketNotesEn = m.PocketNotesEn,
        PocketNotesAr = m.PocketNotesAr,
        DrillNotesEn = m.DrillNotesEn,
        DrillNotesAr = m.DrillNotesAr,
        NotesEn = m.NotesEn,
        NotesAr = m.NotesAr,
        IsPcbOnly = m.IsPcbOnly,
        CreatedAt = m.CreatedAt,
        UpdatedAt = m.UpdatedAt
    };
}
