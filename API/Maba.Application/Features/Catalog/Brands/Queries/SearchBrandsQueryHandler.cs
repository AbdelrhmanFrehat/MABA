using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Brands.Queries;
using Maba.Application.Features.Catalog.Brands.DTOs;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Brands.Handlers;

public class SearchBrandsQueryHandler : IRequestHandler<SearchBrandsQuery, List<BrandDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchBrandsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BrandDto>> Handle(SearchBrandsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Brand>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(b =>
                b.NameEn.ToLower().Contains(searchTerm) ||
                b.NameAr.ToLower().Contains(searchTerm) ||
                (b.DescriptionEn != null && b.DescriptionEn.ToLower().Contains(searchTerm)) ||
                (b.DescriptionAr != null && b.DescriptionAr.ToLower().Contains(searchTerm)));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(b => b.IsActive == request.IsActive.Value);
        }

        var brands = await query
            .OrderBy(b => b.NameEn)
            .ToListAsync(cancellationToken);

        return brands.Select(b => new BrandDto
        {
            Id = b.Id,
            NameEn = b.NameEn,
            NameAr = b.NameAr,
            IsActive = b.IsActive,
            LogoId = b.LogoId,
            WebsiteUrl = b.WebsiteUrl,
            Country = b.Country,
            DescriptionEn = b.DescriptionEn,
            DescriptionAr = b.DescriptionAr,
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt
        }).ToList();
    }
}

