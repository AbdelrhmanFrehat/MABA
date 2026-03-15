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
            .Include(m => m.AvailableColors.Where(c => c.IsActive).OrderBy(c => c.SortOrder))
            .ToListAsync(cancellationToken);

        return materials.Select(m => new MaterialDto
        {
            Id = m.Id,
            NameEn = m.NameEn,
            NameAr = m.NameAr,
            PricePerGram = m.PricePerGram,
            Density = m.Density,
            Color = m.Color,
            IsActive = m.IsActive,
            StockQuantity = m.StockQuantity,
            AvailableColors = m.AvailableColors.Select(c => new MaterialColorDto
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

