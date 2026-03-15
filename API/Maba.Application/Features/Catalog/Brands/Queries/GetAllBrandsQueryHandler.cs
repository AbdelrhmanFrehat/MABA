using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Brands.DTOs;
using Maba.Application.Features.Catalog.Brands.Queries;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Brands.Handlers;

public class GetAllBrandsQueryHandler : IRequestHandler<GetAllBrandsQuery, List<BrandDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllBrandsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BrandDto>> Handle(GetAllBrandsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Brand>().AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(b => b.IsActive == request.IsActive.Value);
        }

        var brands = await query.ToListAsync(cancellationToken);

        return brands.Select(b => new BrandDto
        {
            Id = b.Id,
            NameEn = b.NameEn,
            NameAr = b.NameAr,
            IsActive = b.IsActive,
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt
        }).ToList();
    }
}

