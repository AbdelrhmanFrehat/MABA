using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Categories.Queries;
using Maba.Application.Features.Catalog.Categories.DTOs;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Categories.Handlers;

public class GetCategoriesByParentQueryHandler : IRequestHandler<GetCategoriesByParentQuery, List<CategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCategoriesByParentQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> Handle(GetCategoriesByParentQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Category>()
            .Where(c => c.ParentId == request.ParentId);

        var categories = await query
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.NameEn)
            .ToListAsync(cancellationToken);

        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            ParentId = c.ParentId,
            NameEn = c.NameEn,
            NameAr = c.NameAr,
            Slug = c.Slug,
            SortOrder = c.SortOrder,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList();
    }
}

