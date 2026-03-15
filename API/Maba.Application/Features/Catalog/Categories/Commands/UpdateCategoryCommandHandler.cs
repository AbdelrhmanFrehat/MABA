using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Categories.Commands;
using Maba.Application.Features.Catalog.Categories.DTOs;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Categories.Handlers;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Set<Category>()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            throw new KeyNotFoundException("Category not found");
        }

        // Validate parent if provided and different
        if (request.ParentId.HasValue && request.ParentId != category.ParentId)
        {
            if (request.ParentId.Value == request.Id)
            {
                throw new InvalidOperationException("Category cannot be its own parent");
            }

            var parentExists = await _context.Set<Category>()
                .AnyAsync(c => c.Id == request.ParentId.Value, cancellationToken);

            if (!parentExists)
            {
                throw new KeyNotFoundException("Parent category not found");
            }
        }

        category.ParentId = request.ParentId;
        category.NameEn = request.NameEn;
        category.NameAr = request.NameAr;
        category.Slug = request.Slug;
        category.SortOrder = request.SortOrder;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new CategoryDto
        {
            Id = category.Id,
            ParentId = category.ParentId,
            NameEn = category.NameEn,
            NameAr = category.NameAr,
            Slug = category.Slug,
            SortOrder = category.SortOrder,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }
}

