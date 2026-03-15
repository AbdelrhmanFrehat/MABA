using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Items.Queries;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Domain.Catalog;
using Maba.Domain.Media;

namespace Maba.Application.Features.Catalog.Items.Handlers;

public class GetNewItemsQueryHandler : IRequestHandler<GetNewItemsQuery, List<ItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetNewItemsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ItemDto>> Handle(GetNewItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.Set<Item>()
            .Include(i => i.ItemStatus)
            .Include(i => i.Brand)
            .Include(i => i.Category)
            .Include(i => i.ItemTags)
            .ThenInclude(it => it.Tag)
            .Include(i => i.Inventory)
            .Where(i => i.IsNew && i.ItemStatus.Key == "Active")
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
            var dto = GetFeaturedItemsQueryHandler.MapToItemDto(i);
            if (imageByItem.TryGetValue(i.Id, out var url) && !string.IsNullOrEmpty(url))
                dto.PrimaryImageUrl = url;
            return dto;
        }).ToList();

        return dtos;
    }
}

