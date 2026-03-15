using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Laser.DTOs;
using Maba.Domain.Laser;

namespace Maba.Application.Features.Laser.Materials.Commands;

public class UpdateLaserMaterialCommandHandler : IRequestHandler<UpdateLaserMaterialCommand, LaserMaterialDto?>
{
    private readonly IApplicationDbContext _context;

    public UpdateLaserMaterialCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LaserMaterialDto?> Handle(UpdateLaserMaterialCommand request, CancellationToken cancellationToken)
    {
        var (isValid, normalizedType, errorMessage) = LaserMaterialTypeValidator.ValidateAndNormalize(request.Type);
        if (!isValid)
        {
            throw new ArgumentException(errorMessage);
        }

        var material = await _context.Set<LaserMaterial>()
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (material == null) return null;

        material.NameEn = request.NameEn;
        material.NameAr = request.NameAr;
        material.Type = normalizedType;
        material.MinThicknessMm = request.MinThicknessMm;
        material.MaxThicknessMm = request.MaxThicknessMm;
        material.NotesEn = request.NotesEn;
        material.NotesAr = request.NotesAr;
        material.IsMetal = request.IsMetal;
        material.IsActive = request.IsActive;
        material.SortOrder = request.SortOrder;

        await _context.SaveChangesAsync(cancellationToken);

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
