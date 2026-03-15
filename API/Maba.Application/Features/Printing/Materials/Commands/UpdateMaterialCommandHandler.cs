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
        material.Color = request.Color;
        material.IsActive = request.IsActive;
        material.StockQuantity = request.StockQuantity;
        material.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

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
            CreatedAt = material.CreatedAt,
            UpdatedAt = material.UpdatedAt
        };
    }
}
