using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cnc.DTOs;
using Maba.Domain.Cnc;

namespace Maba.Application.Features.Cnc.Materials.Commands;

public class UpdateCncMaterialCommandHandler : IRequestHandler<UpdateCncMaterialCommand, CncMaterialDto?>
{
    private readonly IApplicationDbContext _context;

    public UpdateCncMaterialCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CncMaterialDto?> Handle(UpdateCncMaterialCommand request, CancellationToken cancellationToken)
    {
        var material = await _context.Set<CncMaterial>()
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (material == null)
        {
            return null;
        }

        var type = CncMaterialTypeHelper.Normalize(request.Type);
        CncMaterialTypeHelper.Validate(type);

        material.NameEn = request.NameEn;
        material.NameAr = request.NameAr;
        material.DescriptionEn = request.DescriptionEn;
        material.DescriptionAr = request.DescriptionAr;
        material.Type = type;
        material.MinThicknessMm = request.MinThicknessMm;
        material.MaxThicknessMm = request.MaxThicknessMm;
        material.IsMetal = request.IsMetal;
        material.IsActive = request.IsActive;
        material.SortOrder = request.SortOrder;
        material.AllowCut = request.AllowCut;
        material.AllowEngrave = request.AllowEngrave;
        material.AllowPocket = request.AllowPocket;
        material.AllowDrill = request.AllowDrill;
        material.MaxCutDepthMm = request.MaxCutDepthMm;
        material.MaxEngraveDepthMm = request.MaxEngraveDepthMm;
        material.MaxPocketDepthMm = request.MaxPocketDepthMm;
        material.MaxDrillDepthMm = request.MaxDrillDepthMm;
        material.CutNotesEn = request.CutNotesEn;
        material.CutNotesAr = request.CutNotesAr;
        material.EngraveNotesEn = request.EngraveNotesEn;
        material.EngraveNotesAr = request.EngraveNotesAr;
        material.PocketNotesEn = request.PocketNotesEn;
        material.PocketNotesAr = request.PocketNotesAr;
        material.DrillNotesEn = request.DrillNotesEn;
        material.DrillNotesAr = request.DrillNotesAr;
        material.NotesEn = request.NotesEn;
        material.NotesAr = request.NotesAr;
        material.IsPcbOnly = CncMaterialTypeHelper.IsPcbOnlyFromType(type);
        material.PcbMaterialType = request.PcbMaterialType;
        material.SupportedBoardThicknesses = request.SupportedBoardThicknesses;
        material.SupportsSingleSided = request.SupportsSingleSided;
        material.SupportsDoubleSided = request.SupportsDoubleSided;

        await _context.SaveChangesAsync(cancellationToken);

        return CncMaterialDto.FromEntity(material);
    }
}
