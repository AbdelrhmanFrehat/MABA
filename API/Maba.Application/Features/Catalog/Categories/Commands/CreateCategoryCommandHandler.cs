using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Categories.Commands;
using Maba.Application.Features.Catalog.Categories.DTOs;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Categories.Handlers;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _context;

    public CreateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        // Validate parent if provided
        if (request.ParentId.HasValue)
        {
            var parentExists = await _context.Set<Category>()
                .AnyAsync(c => c.Id == request.ParentId.Value, cancellationToken);

            if (!parentExists)
            {
                throw new KeyNotFoundException("Parent category not found");
            }
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            ParentId = request.ParentId,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            Slug = request.Slug,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };

        _context.Set<Category>().Add(category);
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

