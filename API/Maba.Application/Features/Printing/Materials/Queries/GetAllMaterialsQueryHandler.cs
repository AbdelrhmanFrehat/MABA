using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.DTOs;
using Maba.Application.Features.Printing.Materials.Queries;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.Materials.Handlers;

public class GetAllMaterialsQueryHandler : IRequestHandler<GetAllMaterialsQuery, List<MaterialDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllMaterialsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MaterialDto>> Handle(GetAllMaterialsQuery request, CancellationToken cancellationToken)
    {
        var materials = await _context.Set<Material>()
            .Include(m => m.AvailableColors.OrderBy(c => c.SortOrder))
            .ToListAsync(cancellationToken);

        var spoolTotals = await _context.Set<FilamentSpool>()
            .Where(s => s.IsActive)
            .GroupBy(s => s.MaterialId)
            .Select(g => new { MaterialId = g.Key, Total = g.Sum(s => s.RemainingWeightGrams) })
            .ToDictionaryAsync(x => x.MaterialId, x => x.Total, cancellationToken);

        var colorIdsWithStock = (await _context.Set<FilamentSpool>()
            .Where(s => s.IsActive && s.RemainingWeightGrams > 0 && s.MaterialColorId != null)
            .Select(s => s.MaterialColorId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken))
            .ToHashSet();

        return materials.Select(m => new MaterialDto
        {
            Id = m.Id,
            NameEn = m.NameEn,
            NameAr = m.NameAr,
            PricePerGram = m.PricePerGram,
            Density = m.Density,
            IsActive = m.IsActive,
            TotalStockGrams = spoolTotals.GetValueOrDefault(m.Id, 0),
            AvailableColors = m.AvailableColors
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
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt
        }).ToList();
    }
}

