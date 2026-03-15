using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Brands.DTOs;
using Maba.Application.Features.Catalog.Brands.Queries;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Brands.Handlers;

public class GetBrandByIdQueryHandler : IRequestHandler<GetBrandByIdQuery, BrandDto>
{
    private readonly IApplicationDbContext _context;

    public GetBrandByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BrandDto> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
    {
        var brand = await _context.Set<Brand>()
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (brand == null)
        {
            throw new KeyNotFoundException("Brand not found");
        }

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

