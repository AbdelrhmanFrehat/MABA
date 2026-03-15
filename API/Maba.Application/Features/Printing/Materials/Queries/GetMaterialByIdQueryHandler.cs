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
            .Include(m => m.AvailableColors.Where(c => c.IsActive).OrderBy(c => c.SortOrder))
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (material == null)
        {
            return null;
        }

        return new MaterialDto
        {
            Id = material.Id,
            NameEn = material.NameEn,
            NameAr = material.NameAr,
            PricePerGram = material.PricePerGram,
            Density = material.Density,
            Color = material.Color,
            IsActive = material.IsActive,
            StockQuantity = material.StockQuantity,
            AvailableColors = material.AvailableColors.Select(c => new MaterialColorDto
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
