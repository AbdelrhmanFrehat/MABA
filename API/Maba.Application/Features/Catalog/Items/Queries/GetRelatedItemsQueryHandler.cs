using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Items.Queries;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Domain.Catalog;
using Maba.Domain.Media;

namespace Maba.Application.Features.Catalog.Items.Handlers;

public class GetRelatedItemsQueryHandler : IRequestHandler<GetRelatedItemsQuery, List<ItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRelatedItemsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ItemDto>> Handle(GetRelatedItemsQuery request, CancellationToken cancellationToken)
    {
        var item = await _context.Set<Item>()
            .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);

        if (item == null)
            throw new KeyNotFoundException("Item not found.");

        // Get related items by same category or brand
        var relatedItems = await _context.Set<Item>()
            .Include(i => i.ItemStatus)
            .Include(i => i.Brand)
            .Include(i => i.Category)
            .Include(i => i.ItemTags)
                .ThenInclude(it => it.Tag)
            .Include(i => i.Inventory)
            .Where(i => i.Id != request.ItemId &&
                        i.ItemStatus.Key == "Active" &&
                        (i.CategoryId == item.CategoryId || i.BrandId == item.BrandId))
            .OrderByDescending(i => i.CreatedAt)
            .Take(request.Limit ?? 10)
            .ToListAsync(cancellationToken);

        if (relatedItems.Count == 0)
            return new List<ItemDto>();

        // Load primary image for each related item (same logic as GetFeaturedItemsQueryHandler)
        var itemIds = relatedItems.Select(i => i.Id).ToList();
        var imageLinks = await _context.Set<EntityMediaLink>()
            .Include(eml => eml.MediaAsset)
                .ThenInclude(ma => ma.MediaType)
            .Where(eml => eml.EntityType == "Item" &&
                          itemIds.Contains(eml.EntityId) &&
                          eml.MediaAsset.MediaType.Key == "Image")
            .OrderByDescending(eml => eml.IsPrimary)
            .ThenBy(eml => eml.SortOrder)
            .ToListAsync(cancellationToken);

        var imageByItem = imageLinks
            .GroupBy(eml => eml.EntityId)
            .ToDictionary(g => g.Key, g => g.First().MediaAsset?.FileUrl);

        return relatedItems.Select(i =>
        {
            var dto = GetFeaturedItemsQueryHandler.MapToItemDto(i);
            if (imageByItem.TryGetValue(i.Id, out var url) && !string.IsNullOrEmpty(url))
                dto.PrimaryImageUrl = url;
            return dto;
        }).ToList();
    }
}

