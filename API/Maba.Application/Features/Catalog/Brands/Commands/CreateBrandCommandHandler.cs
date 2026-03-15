using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Brands.Commands;
using Maba.Application.Features.Catalog.Brands.DTOs;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Brands.Handlers;

public class CreateBrandCommandHandler : IRequestHandler<CreateBrandCommand, BrandDto>
{
    private readonly IApplicationDbContext _context;

    public CreateBrandCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BrandDto> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = new Brand
        {
            Id = Guid.NewGuid(),
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            IsActive = request.IsActive
        };

        _context.Set<Brand>().Add(brand);
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

