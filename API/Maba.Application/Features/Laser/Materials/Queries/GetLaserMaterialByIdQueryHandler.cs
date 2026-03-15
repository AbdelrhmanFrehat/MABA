using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Laser.DTOs;
using Maba.Domain.Laser;

namespace Maba.Application.Features.Laser.Materials.Queries;

public class GetLaserMaterialByIdQueryHandler : IRequestHandler<GetLaserMaterialByIdQuery, LaserMaterialDto?>
{
    private readonly IApplicationDbContext _context;

    public GetLaserMaterialByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LaserMaterialDto?> Handle(GetLaserMaterialByIdQuery request, CancellationToken cancellationToken)
    {
        var material = await _context.Set<LaserMaterial>()
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (material == null) return null;

        return new LaserMaterialDto
        {
            Id = material.Id,
            NameEn = material.NameEn,
            NameAr = material.NameAr,
            Type = material.Type,
            MinThicknessMm = material.MinThicknessMm,
            MaxThicknessMm = material.MaxThicknessMm,
            NotesEn = material.NotesEn,
            NotesAr = material.NotesAr,
            IsMetal = material.IsMetal,
            IsActive = material.IsActive,
            SortOrder = material.SortOrder,
            CreatedAt = material.CreatedAt,
            UpdatedAt = material.UpdatedAt
        };
    }
}
