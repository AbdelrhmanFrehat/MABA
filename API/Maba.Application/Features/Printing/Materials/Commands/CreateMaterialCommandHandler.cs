using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.Materials.Commands;
using Maba.Application.Features.Printing.DTOs;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.Materials.Handlers;

public class CreateMaterialCommandHandler : IRequestHandler<CreateMaterialCommand, MaterialDto>
{
    private readonly IApplicationDbContext _context;

    public CreateMaterialCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MaterialDto> Handle(CreateMaterialCommand request, CancellationToken cancellationToken)
    {
        var material = new Material
        {
            Id = Guid.NewGuid(),
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            PricePerGram = request.PricePerGram,
            Density = request.Density
        };

        _context.Set<Material>().Add(material);
        await _context.SaveChangesAsync(cancellationToken);

        return new MaterialDto
        {
            Id = material.Id,
            NameEn = material.NameEn,
            NameAr = material.NameAr,
            PricePerGram = material.PricePerGram,
            Density = material.Density,
            TotalStockGrams = 0,
            CreatedAt = material.CreatedAt,
            UpdatedAt = material.UpdatedAt
        };
    }
}

