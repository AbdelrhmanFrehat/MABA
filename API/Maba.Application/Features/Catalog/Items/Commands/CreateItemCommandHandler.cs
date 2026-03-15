using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Items.Commands;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Items.Handlers;

public class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, ItemDto>
{
    private readonly IApplicationDbContext _context;

    public CreateItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ItemDto> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        // Validate SKU uniqueness
        var existingSku = await _context.Set<Item>()
            .AnyAsync(i => i.Sku == request.Sku, cancellationToken);

        if (existingSku)
        {
            throw new InvalidOperationException("Item with this SKU already exists");
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

        var item = new Item
        {
            Id = Guid.NewGuid(),
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            Sku = request.Sku,
            GeneralDescriptionEn = request.GeneralDescriptionEn,
            GeneralDescriptionAr = request.GeneralDescriptionAr,
            ItemStatusId = request.ItemStatusId,
            Price = request.Price,
            Currency = request.Currency,
            BrandId = request.BrandId,
            CategoryId = request.CategoryId,
            AverageRating = 0,
            ReviewsCount = 0,
            ViewsCount = 0
        };

        _context.Set<Item>().Add(item);

        // Add tags
        if (request.TagIds.Any())
        {
            foreach (var tagId in request.TagIds)
            {
                _context.Set<ItemTag>().Add(new ItemTag
                {
                    ItemId = item.Id,
                    TagId = tagId
                });
            }
        }

        // Create inventory if initial quantity provided
        if (request.InitialQuantity.HasValue)
        {
            var inventory = new Maba.Domain.Catalog.Inventory
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                QuantityOnHand = request.InitialQuantity.Value,
                ReorderLevel = request.ReorderLevel,
                LastStockInAt = DateTime.UtcNow
            };
            _context.Set<Maba.Domain.Catalog.Inventory>().Add(inventory);
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

        return MapToDto(itemWithRelations!);
    }

    public static ItemDto MapToDto(Item item)
    {
        return new ItemDto
        {
            Id = item.Id,
            NameEn = item.NameEn,
            NameAr = item.NameAr,
            Sku = item.Sku,
            GeneralDescriptionEn = item.GeneralDescriptionEn,
            GeneralDescriptionAr = item.GeneralDescriptionAr,
            ItemStatusId = item.ItemStatusId,
            ItemStatusKey = item.ItemStatus.Key,
            Price = item.Price,
            Currency = item.Currency,
            BrandId = item.BrandId,
            BrandNameEn = item.Brand?.NameEn,
            CategoryId = item.CategoryId,
            CategoryNameEn = item.Category?.NameEn,
            AverageRating = item.AverageRating,
            ReviewsCount = item.ReviewsCount,
            ViewsCount = item.ViewsCount,
            TagIds = item.ItemTags.Select(it => it.TagId).ToList(),
            Inventory = item.Inventory != null ? new InventoryDto
            {
                Id = item.Inventory.Id,
                ItemId = item.Inventory.ItemId,
                QuantityOnHand = item.Inventory.QuantityOnHand,
                ReorderLevel = item.Inventory.ReorderLevel,
                LastStockInAt = item.Inventory.LastStockInAt
            } : null,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }
}

