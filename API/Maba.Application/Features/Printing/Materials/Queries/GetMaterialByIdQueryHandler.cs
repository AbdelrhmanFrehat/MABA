using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.DTOs;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.Materials.Queries;

public class GetMaterialByIdQueryHandler : IRequestHandler<GetMaterialByIdQuery, MaterialDto?>
{
    private readonly IApplicationDbContext _context;

    public GetMaterialByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MaterialDto?> Handle(GetMaterialByIdQuery request, CancellationToken cancellationToken)
    {
        var material = await _context.Set<Material>()
            .Include(m => m.AvailableColors.OrderBy(c => c.SortOrder))
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (material == null) return null;

        var totalStock = await _context.Set<FilamentSpool>()
            .Where(s => s.MaterialId == material.Id && s.IsActive)
            .SumAsync(s => s.RemainingWeightGrams, cancellationToken);

        var colorIdsWithStock = (await _context.Set<FilamentSpool>()
            .Where(s => s.MaterialId == material.Id && s.IsActive && s.RemainingWeightGrams > 0 && s.MaterialColorId != null)
            .Select(s => s.MaterialColorId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken))
            .ToHashSet();

        return new MaterialDto
        {
            Id = material.Id,
            NameEn = material.NameEn,
            NameAr = material.NameAr,
            PricePerGram = material.PricePerGram,
            Density = material.Density,
            IsActive = material.IsActive,
            TotalStockGrams = totalStock,
            AvailableColors = material.AvailableColors
                .Where(c => c.IsActive && colorIdsWithStock.Contains(c.Id))
                .Select(c => new MaterialColorDto
                {
                    Id = c.Id,
                    MaterialId = c.MaterialId,
                    NameEn = c.NameEn,
                    NameAr = c.NameAr,
                    HexCode = c.HexCode,
                    IsActive = c.IsActive,
                    SortOrder = c.SortOrder,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }).ToList(),
            CreatedAt = material.CreatedAt,
            UpdatedAt = material.UpdatedAt
        };
    }
}
