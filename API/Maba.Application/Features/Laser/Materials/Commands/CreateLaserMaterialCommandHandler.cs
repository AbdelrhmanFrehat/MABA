using MediatR;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Laser.DTOs;
using Maba.Domain.Laser;

namespace Maba.Application.Features.Laser.Materials.Commands;

public class CreateLaserMaterialCommandHandler : IRequestHandler<CreateLaserMaterialCommand, LaserMaterialDto>
{
    private readonly IApplicationDbContext _context;

    public CreateLaserMaterialCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LaserMaterialDto> Handle(CreateLaserMaterialCommand request, CancellationToken cancellationToken)
    {
        var (isValid, normalizedType, errorMessage) = LaserMaterialTypeValidator.ValidateAndNormalize(request.Type);
        if (!isValid)
        {
            throw new ArgumentException(errorMessage);
        }

        var material = new LaserMaterial
        {
            Id = Guid.NewGuid(),
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            Type = normalizedType,
            MinThicknessMm = request.MinThicknessMm,
            MaxThicknessMm = request.MaxThicknessMm,
            NotesEn = request.NotesEn,
            NotesAr = request.NotesAr,
            IsMetal = request.IsMetal,
            IsActive = request.IsActive,
            SortOrder = request.SortOrder
        };

        _context.Set<LaserMaterial>().Add(material);
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
