using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Items.Queries;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Domain.Catalog;

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
        {
            throw new KeyNotFoundException("Item not found.");
        }

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

        return relatedItems.Select(i => GetFeaturedItemsQueryHandler.MapToItemDto(i)).ToList();
    }
}

