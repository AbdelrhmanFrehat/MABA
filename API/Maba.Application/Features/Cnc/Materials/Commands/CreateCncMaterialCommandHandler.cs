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
        var type = CncMaterialTypeHelper.Normalize(request.Type);
        CncMaterialTypeHelper.Validate(type);

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
            IsPcbOnly = CncMaterialTypeHelper.IsPcbOnlyFromType(type),
            PcbMaterialType = request.PcbMaterialType,
            SupportedBoardThicknesses = request.SupportedBoardThicknesses,
            SupportsSingleSided = request.SupportsSingleSided,
            SupportsDoubleSided = request.SupportsDoubleSided
        };

        _context.Set<CncMaterial>().Add(material);
        await _context.SaveChangesAsync(cancellationToken);

        return CncMaterialDto.FromEntity(material);
    }
}
