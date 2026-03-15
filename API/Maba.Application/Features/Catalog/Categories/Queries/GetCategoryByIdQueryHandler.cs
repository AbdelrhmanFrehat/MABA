using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Categories.DTOs;
using Maba.Application.Features.Catalog.Categories.Queries;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Categories.Handlers;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly IApplicationDbContext _context;

    public GetCategoryByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _context.Set<Category>()
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            throw new KeyNotFoundException("Category not found");
        }

        return new CategoryDto
        {
            Id = category.Id,
            ParentId = category.ParentId,
            NameEn = category.NameEn,
            NameAr = category.NameAr,
            Slug = category.Slug,
            SortOrder = category.SortOrder,
            IsActive = category.IsActive,
            Children = category.Children.Select(ch => new CategoryDto
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
            }).ToList(),
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }
}

