using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Items.Commands;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Items.Handlers;

public class UpdateItemCommandHandler : IRequestHandler<UpdateItemCommand, ItemDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ItemDto> Handle(UpdateItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.Set<Item>()
            .Include(i => i.ItemTags)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (item == null)
        {
            throw new KeyNotFoundException("Item not found");
        }

        // Validate SKU uniqueness if changed
        if (item.Sku != request.Sku)
        {
            var existingSku = await _context.Set<Item>()
                .AnyAsync(i => i.Sku == request.Sku && i.Id != request.Id, cancellationToken);

            if (existingSku)
            {
                throw new InvalidOperationException("Item with this SKU already exists");
            }
        }

        // Validate item status
        var itemStatus = await _context.Set<ItemStatus>()
            .FirstOrDefaultAsync(s => s.Id == request.ItemStatusId, cancellationToken);

        if (itemStatus == null)
        {
            throw new KeyNotFoundException("Item status not found");
        }

        // Validate brand if provided
        if (request.BrandId.HasValue)
        {
            var brandExists = await _context.Set<Brand>()
                .AnyAsync(b => b.Id == request.BrandId.Value, cancellationToken);

            if (!brandExists)
            {
                throw new KeyNotFoundException("Brand not found");
            }
        }

        // Validate category if provided
        if (request.CategoryId.HasValue)
        {
            var categoryExists = await _context.Set<Category>()
                .AnyAsync(c => c.Id == request.CategoryId.Value, cancellationToken);

            if (!categoryExists)
            {
                throw new KeyNotFoundException("Category not found");
            }
        }

        // Validate tags
        if (request.TagIds.Any())
        {
            var tagCount = await _context.Set<Tag>()
                .CountAsync(t => request.TagIds.Contains(t.Id), cancellationToken);

            if (tagCount != request.TagIds.Count)
            {
                throw new KeyNotFoundException("One or more tags not found");
            }
        }

        // Update item
        item.NameEn = request.NameEn;
        item.NameAr = request.NameAr;
        item.Sku = request.Sku;
        item.GeneralDescriptionEn = request.GeneralDescriptionEn;
        item.GeneralDescriptionAr = request.GeneralDescriptionAr;
        item.ItemStatusId = request.ItemStatusId;
        item.Price = request.Price;
        item.Currency = request.Currency;
        item.BrandId = request.BrandId;
        item.CategoryId = request.CategoryId;
        item.UpdatedAt = DateTime.UtcNow;

        // Update tags
        var existingTagIds = item.ItemTags.Select(it => it.TagId).ToList();
        var tagsToAdd = request.TagIds.Except(existingTagIds).ToList();
        var tagsToRemove = existingTagIds.Except(request.TagIds).ToList();

        // Remove tags
        if (tagsToRemove.Any())
        {
            var itemTagsToRemove = item.ItemTags
                .Where(it => tagsToRemove.Contains(it.TagId))
                .ToList();

            foreach (var it in itemTagsToRemove)
            {
                _context.Set<ItemTag>().Remove(it);
            }
        }

        // Add new tags
        if (tagsToAdd.Any())
        {
            foreach (var tagId in tagsToAdd)
            {
                _context.Set<ItemTag>().Add(new ItemTag
                {
                    ItemId = item.Id,
                    TagId = tagId
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Load item with relations for response
        var itemWithRelations = await _context.Set<Item>()
            .Include(i => i.ItemStatus)
            .Include(i => i.Brand)
            .Include(i => i.Category)
            .Include(i => i.ItemTags)
            .ThenInclude(it => it.Tag)
            .Include(i => i.Inventory)
            .FirstOrDefaultAsync(i => i.Id == item.Id, cancellationToken);

        return CreateItemCommandHandler.MapToDto(itemWithRelations!);
    }
}

