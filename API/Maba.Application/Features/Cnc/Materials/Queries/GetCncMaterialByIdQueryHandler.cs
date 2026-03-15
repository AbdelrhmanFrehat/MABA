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

        if (material == null)
        {
            return null;
        }

        return new CncMaterialDto
        {
            Id = material.Id,
            NameEn = material.NameEn,
            NameAr = material.NameAr,
            DescriptionEn = material.DescriptionEn,
            DescriptionAr = material.DescriptionAr,
            Type = material.Type,
            MinThicknessMm = material.MinThicknessMm,
            MaxThicknessMm = material.MaxThicknessMm,
            IsMetal = material.IsMetal,
            IsActive = material.IsActive,
            SortOrder = material.SortOrder,
            AllowCut = material.AllowCut,
            AllowEngrave = material.AllowEngrave,
            AllowPocket = material.AllowPocket,
            AllowDrill = material.AllowDrill,
            MaxCutDepthMm = material.MaxCutDepthMm,
            MaxEngraveDepthMm = material.MaxEngraveDepthMm,
            MaxPocketDepthMm = material.MaxPocketDepthMm,
            MaxDrillDepthMm = material.MaxDrillDepthMm,
            CutNotesEn = material.CutNotesEn,
            CutNotesAr = material.CutNotesAr,
            EngraveNotesEn = material.EngraveNotesEn,
            EngraveNotesAr = material.EngraveNotesAr,
            PocketNotesEn = material.PocketNotesEn,
            PocketNotesAr = material.PocketNotesAr,
            DrillNotesEn = material.DrillNotesEn,
            DrillNotesAr = material.DrillNotesAr,
            NotesEn = material.NotesEn,
            NotesAr = material.NotesAr,
            IsPcbOnly = material.IsPcbOnly,
            CreatedAt = material.CreatedAt,
            UpdatedAt = material.UpdatedAt
        };
    }
}
