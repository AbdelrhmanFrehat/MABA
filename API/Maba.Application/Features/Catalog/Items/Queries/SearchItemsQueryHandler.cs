using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Common.Models;
using Maba.Application.Features.Catalog.Items.Queries;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Domain.Catalog;
using Maba.Domain.Media;

namespace Maba.Application.Features.Catalog.Items.Handlers;

public class SearchItemsQueryHandler : IRequestHandler<SearchItemsQuery, PagedResult<ItemDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchItemsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ItemDto>> Handle(SearchItemsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Item>()
            .Include(i => i.ItemStatus)
            .Include(i => i.Brand)
            .Include(i => i.Category)
            .Include(i => i.ItemTags)
            .ThenInclude(it => it.Tag)
            .Include(i => i.Inventory)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(i =>
                i.NameEn.ToLower().Contains(searchTerm) ||
                i.NameAr.ToLower().Contains(searchTerm) ||
                i.Sku.ToLower().Contains(searchTerm) ||
                (i.GeneralDescriptionEn != null && i.GeneralDescriptionEn.ToLower().Contains(searchTerm)) ||
                (i.GeneralDescriptionAr != null && i.GeneralDescriptionAr.ToLower().Contains(searchTerm)));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(i => i.CategoryId == request.CategoryId.Value);
        }

        if (request.BrandId.HasValue)
        {
            query = query.Where(i => i.BrandId == request.BrandId.Value);
        }

        if (request.ItemStatusId.HasValue)
        {
            query = query.Where(i => i.ItemStatusId == request.ItemStatusId.Value);
        }

        if (request.TagId.HasValue)
        {
            query = query.Where(i => i.ItemTags.Any(it => it.TagId == request.TagId.Value));
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(i => i.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(i => i.Price <= request.MaxPrice.Value);
        }

        if (request.IsFeatured.HasValue)
        {
            query = query.Where(i => i.IsFeatured == request.IsFeatured.Value);
        }

        if (request.IsNew.HasValue)
        {
            query = query.Where(i => i.IsNew == request.IsNew.Value);
        }

        if (request.IsOnSale.HasValue)
        {
            query = query.Where(i => i.IsOnSale == request.IsOnSale.Value);
        }

        if (request.InStock == true)
        {
            query = query.Where(i => i.Inventory != null && (i.Inventory.QuantityOnHand - i.Inventory.QuantityReserved) > 0);
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "price" => request.SortDescending
                ? query.OrderByDescending(i => i.Price)
                : query.OrderBy(i => i.Price),
            "rating" => request.SortDescending
                ? query.OrderByDescending(i => i.AverageRating)
                : query.OrderBy(i => i.AverageRating),
            "views" => request.SortDescending
                ? query.OrderByDescending(i => i.ViewsCount)
                : query.OrderBy(i => i.ViewsCount),
            "createdat" => request.SortDescending
                ? query.OrderByDescending(i => i.CreatedAt)
                : query.OrderBy(i => i.CreatedAt),
            _ => query.OrderByDescending(i => i.CreatedAt)
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var itemIds = items.Select(i => i.Id).ToList();
        var primaryImages = await _context.Set<EntityMediaLink>()
            .Include(eml => eml.MediaAsset)
            .Where(eml => eml.EntityType == "Item" && itemIds.Contains(eml.EntityId) && eml.IsPrimary)
            .ToDictionaryAsync(eml => eml.EntityId, eml => eml.MediaAsset.FileUrl, cancellationToken);

        // For items with no primary image, use first image by sort order
        var allLinks = await _context.Set<EntityMediaLink>()
            .Include(eml => eml.MediaAsset)
            .Where(eml => eml.EntityType == "Item" && itemIds.Contains(eml.EntityId))
            .OrderBy(eml => eml.SortOrder)
            .ToListAsync(cancellationToken);
        var firstImages = allLinks
            .GroupBy(eml => eml.EntityId)
            .ToDictionary(g => g.Key, g => g.First().MediaAsset.FileUrl);

        var itemsDto = items.Select(i =>
        {
            var primaryUrl = primaryImages.GetValueOrDefault(i.Id) ?? firstImages.GetValueOrDefault(i.Id);
            return new ItemDto
            {
                Id = i.Id,
                NameEn = i.NameEn,
                NameAr = i.NameAr,
                Sku = i.Sku,
                GeneralDescriptionEn = i.GeneralDescriptionEn,
                GeneralDescriptionAr = i.GeneralDescriptionAr,
                ItemStatusId = i.ItemStatusId,
                ItemStatusKey = i.ItemStatus.Key,
                Price = i.Price,
                DiscountPrice = i.DiscountPrice,
                Currency = i.Currency,
                BrandId = i.BrandId,
                BrandNameEn = i.Brand?.NameEn,
                CategoryId = i.CategoryId,
                CategoryNameEn = i.Category?.NameEn,
                AverageRating = i.AverageRating,
                ReviewsCount = i.ReviewsCount,
                ViewsCount = i.ViewsCount,
                IsFeatured = i.IsFeatured,
                IsNew = i.IsNew,
                IsOnSale = i.IsOnSale,
                TagIds = i.ItemTags.Select(it => it.TagId).ToList(),
                Inventory = i.Inventory != null ? new InventoryDto
                {
                    Id = i.Inventory.Id,
                    ItemId = i.Inventory.ItemId,
                    QuantityOnHand = i.Inventory.QuantityOnHand,
                    QuantityReserved = i.Inventory.QuantityReserved,
                    QuantityAvailable = i.Inventory.QuantityAvailable,
                    QuantityOnOrder = i.Inventory.QuantityOnOrder,
                    ReorderLevel = i.Inventory.ReorderLevel,
                    CostPerUnit = i.Inventory.CostPerUnit,
                    LastStockInAt = i.Inventory.LastStockInAt,
                    LastStockOutAt = i.Inventory.LastStockOutAt
                } : null,
                PrimaryImageUrl = primaryUrl,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            };
        }).ToList();

        return new PagedResult<ItemDto>
        {
            Items = itemsDto,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

