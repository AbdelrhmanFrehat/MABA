using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Categories.DTOs;
using Maba.Application.Features.Catalog.Categories.Queries;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Categories.Handlers;

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, List<CategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Category>().AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.IsActive.Value);
        }

        if (request.IncludeChildren)
        {
            query = query.Include(c => c.Children);
        }

        var categories = await query.ToListAsync(cancellationToken);

        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            ParentId = c.ParentId,
            NameEn = c.NameEn,
            NameAr = c.NameAr,
            Slug = c.Slug,
            SortOrder = c.SortOrder,
            IsActive = c.IsActive,
            Children = request.IncludeChildren ? c.Children.Select(ch => new CategoryDto
            {
                Id = ch.Id,
                ParentId = ch.ParentId,
                NameEn = ch.NameEn,
                NameAr = ch.NameAr,
                Slug = ch.Slug,
                SortOrder = ch.SortOrder,
                IsActive = ch.IsActive,
                CreatedAt = ch.CreatedAt,
                UpdatedAt = ch.UpdatedAt
            }).ToList() : new List<CategoryDto>(),
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList();
    }
}

