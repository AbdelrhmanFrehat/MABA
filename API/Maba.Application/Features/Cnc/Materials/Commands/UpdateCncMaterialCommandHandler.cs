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

        var validTypes = new[] { "routing", "pcb" };
        var type = request.Type?.ToLowerInvariant() ?? "routing";
        if (!validTypes.Contains(type))
        {
            throw new ArgumentException($"Invalid material type '{request.Type}'. Valid types: routing, pcb");
        }

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
        material.IsPcbOnly = request.IsPcbOnly;

        await _context.SaveChangesAsync(cancellationToken);

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
