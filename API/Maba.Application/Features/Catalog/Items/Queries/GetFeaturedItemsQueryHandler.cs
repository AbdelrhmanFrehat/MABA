using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Items.Queries;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Domain.Catalog;
using Maba.Domain.Media;

namespace Maba.Application.Features.Catalog.Items.Handlers;

public class GetFeaturedItemsQueryHandler : IRequestHandler<GetFeaturedItemsQuery, List<ItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFeaturedItemsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ItemDto>> Handle(GetFeaturedItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.Set<Item>()
            .Include(i => i.ItemStatus)
            .Include(i => i.Brand)
            .Include(i => i.Category)
            .Include(i => i.ItemTags)
            .ThenInclude(it => it.Tag)
            .Include(i => i.Inventory)
            .Where(i => i.IsFeatured && i.ItemStatus.Key == "Active")
            .OrderByDescending(i => i.CreatedAt)
            .Take(request.Limit ?? 10)
            .ToListAsync(cancellationToken);

        var itemIds = items.Select(i => i.Id).ToList();
        if (itemIds.Count == 0)
            return new List<ItemDto>();

        var allLinks = await _context.Set<EntityMediaLink>()
            .Include(eml => eml.MediaAsset)
            .ThenInclude(ma => ma.MediaType)
            .Where(eml => eml.EntityType == "Item" && itemIds.Contains(eml.EntityId))
            .OrderByDescending(eml => eml.IsPrimary)
            .ThenBy(eml => eml.SortOrder)
            .ToListAsync(cancellationToken);

        // Only use Image type for primary image URL (exclude Document, Video, etc.)
        var imageLinks = allLinks
            .Where(eml => eml.MediaAsset?.MediaType?.Key == "Image")
            .ToList();
        var imageByItem = imageLinks
            .GroupBy(eml => eml.EntityId)
            .ToDictionary(g => g.Key, g => g.First().MediaAsset?.FileUrl);

        var dtos = items.Select(i =>
        {
            var dto = MapToItemDto(i);
            if (imageByItem.TryGetValue(i.Id, out var url) && !string.IsNullOrEmpty(url))
                dto.PrimaryImageUrl = url;
            return dto;
        }).ToList();

        return dtos;
    }

    public static ItemDto MapToItemDto(Item i)
    {
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
            Weight = i.Weight,
            Dimensions = i.Dimensions,
            TaxRate = i.TaxRate,
            IsFeatured = i.IsFeatured,
            IsNew = i.IsNew,
            IsOnSale = i.IsOnSale,
            MinOrderQuantity = i.MinOrderQuantity,
            MaxOrderQuantity = i.MaxOrderQuantity,
            WarrantyPeriodMonths = i.WarrantyPeriodMonths,
            MetaTitleEn = i.MetaTitleEn,
            MetaTitleAr = i.MetaTitleAr,
            MetaDescriptionEn = i.MetaDescriptionEn,
            MetaDescriptionAr = i.MetaDescriptionAr,
            BrandId = i.BrandId,
            BrandNameEn = i.Brand?.NameEn,
            CategoryId = i.CategoryId,
            CategoryNameEn = i.Category?.NameEn,
            AverageRating = i.AverageRating,
            ReviewsCount = i.ReviewsCount,
            ViewsCount = i.ViewsCount,
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
            CreatedAt = i.CreatedAt,
            UpdatedAt = i.UpdatedAt
        };
    }
}

