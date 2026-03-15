using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Categories.Queries;
using Maba.Application.Features.Catalog.Categories.DTOs;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Categories.Handlers;

public class GetCategoryTreeQueryHandler : IRequestHandler<GetCategoryTreeQuery, List<CategoryTreeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCategoryTreeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryTreeDto>> Handle(GetCategoryTreeQuery request, CancellationToken cancellationToken)
    {
        var categories = await _context.Set<Category>()
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.NameEn)
            .ToListAsync(cancellationToken);

        // Build tree structure
        var categoryDict = categories.ToDictionary(c => c.Id, c => new CategoryTreeDto
        {
            Id = c.Id,
            ParentId = c.ParentId,
            NameEn = c.NameEn,
            NameAr = c.NameAr,
            Slug = c.Slug,
            SortOrder = c.SortOrder,
            IsActive = c.IsActive
        });

        var rootCategories = new List<CategoryTreeDto>();

        foreach (var category in categoryDict.Values)
        {
            if (category.ParentId == null)
            {
                rootCategories.Add(category);
            }
            else if (categoryDict.TryGetValue(category.ParentId.Value, out var parent))
            {
                parent.Children.Add(category);
            }
        }

        return rootCategories.OrderBy(c => c.SortOrder).ToList();
    }
}

