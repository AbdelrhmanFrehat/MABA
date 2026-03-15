using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Items.Queries;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Items.Handlers;

public class GetOnSaleItemsQueryHandler : IRequestHandler<GetOnSaleItemsQuery, List<ItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOnSaleItemsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ItemDto>> Handle(GetOnSaleItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.Set<Item>()
            .Include(i => i.ItemStatus)
            .Include(i => i.Brand)
            .Include(i => i.Category)
            .Include(i => i.ItemTags)
            .ThenInclude(it => it.Tag)
            .Include(i => i.Inventory)
            .Where(i => i.IsOnSale && i.ItemStatus.Key == "Active")
            .OrderByDescending(i => i.CreatedAt)
            .Take(request.Limit ?? 10)
            .ToListAsync(cancellationToken);

        return items.Select(i => GetFeaturedItemsQueryHandler.MapToItemDto(i)).ToList();
    }
}

