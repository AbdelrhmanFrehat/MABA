using MediatR;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cnc.DTOs;
using Maba.Domain.Cnc;

namespace Maba.Application.Features.Cnc.Materials.Commands;

public class CreateCncMaterialCommandHandler : IRequestHandler<CreateCncMaterialCommand, CncMaterialDto>
{
    private readonly IApplicationDbContext _context;

    public CreateCncMaterialCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CncMaterialDto> Handle(CreateCncMaterialCommand request, CancellationToken cancellationToken)
    {
        var validTypes = new[] { "routing", "pcb" };
        var type = request.Type?.ToLowerInvariant() ?? "routing";
        if (!validTypes.Contains(type))
        {
            throw new ArgumentException($"Invalid material type '{request.Type}'. Valid types: routing, pcb");
        }

        var material = new CncMaterial
        {
            Id = Guid.NewGuid(),
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            DescriptionEn = request.DescriptionEn,
            DescriptionAr = request.DescriptionAr,
            Type = type,
            MinThicknessMm = request.MinThicknessMm,
            MaxThicknessMm = request.MaxThicknessMm,
            IsMetal = request.IsMetal,
            IsActive = request.IsActive,
            SortOrder = request.SortOrder,
            AllowCut = request.AllowCut,
            AllowEngrave = request.AllowEngrave,
            AllowPocket = request.AllowPocket,
            AllowDrill = request.AllowDrill,
            MaxCutDepthMm = request.MaxCutDepthMm,
            MaxEngraveDepthMm = request.MaxEngraveDepthMm,
            MaxPocketDepthMm = request.MaxPocketDepthMm,
            MaxDrillDepthMm = request.MaxDrillDepthMm,
            CutNotesEn = request.CutNotesEn,
            CutNotesAr = request.CutNotesAr,
            EngraveNotesEn = request.EngraveNotesEn,
            EngraveNotesAr = request.EngraveNotesAr,
            PocketNotesEn = request.PocketNotesEn,
            PocketNotesAr = request.PocketNotesAr,
            DrillNotesEn = request.DrillNotesEn,
            DrillNotesAr = request.DrillNotesAr,
            NotesEn = request.NotesEn,
            NotesAr = request.NotesAr,
            IsPcbOnly = request.IsPcbOnly
        };

        _context.Set<CncMaterial>().Add(material);
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
