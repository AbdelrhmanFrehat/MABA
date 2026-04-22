using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.DTOs;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.Materials.Commands;

public class UpdateMaterialCommandHandler : IRequestHandler<UpdateMaterialCommand, MaterialDto?>
{
    private readonly IApplicationDbContext _context;

    public UpdateMaterialCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MaterialDto?> Handle(UpdateMaterialCommand request, CancellationToken cancellationToken)
    {
        var material = await _context.Set<Material>()
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (material == null)
        {
            return null;
        }

        material.NameEn = request.NameEn;
        material.NameAr = request.NameAr;
        material.PricePerGram = request.PricePerGram;
        material.Density = request.Density;
        material.IsActive = request.IsActive;
        material.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var totalStock = await _context.Set<FilamentSpool>()
            .Where(s => s.MaterialId == material.Id && s.IsActive)
            .SumAsync(s => s.RemainingWeightGrams, CancellationToken.None);

        return new MaterialDto
        {
            Id = material.Id,
            NameEn = material.NameEn,
            NameAr = material.NameAr,
            PricePerGram = material.PricePerGram,
            Density = material.Density,
            IsActive = material.IsActive,
            TotalStockGrams = totalStock,
            CreatedAt = material.CreatedAt,
            UpdatedAt = material.UpdatedAt
        };
    }
}
