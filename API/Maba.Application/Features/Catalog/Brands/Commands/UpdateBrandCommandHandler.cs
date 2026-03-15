using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Brands.Commands;
using Maba.Application.Features.Catalog.Brands.DTOs;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Brands.Handlers;

public class UpdateBrandCommandHandler : IRequestHandler<UpdateBrandCommand, BrandDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateBrandCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BrandDto> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await _context.Set<Brand>()
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (brand == null)
        {
            throw new KeyNotFoundException("Brand not found");
        }

        brand.NameEn = request.NameEn;
        brand.NameAr = request.NameAr;
        brand.IsActive = request.IsActive;
        brand.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new BrandDto
        {
            Id = brand.Id,
            NameEn = brand.NameEn,
            NameAr = brand.NameAr,
            IsActive = brand.IsActive,
            CreatedAt = brand.CreatedAt,
            UpdatedAt = brand.UpdatedAt
        };
    }
}

