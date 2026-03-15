using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Inventory.Queries;
using Maba.Domain.Catalog;
using InventoryEntity = Maba.Domain.Catalog.Inventory;

namespace Maba.Application.Features.Catalog.Inventory.Handlers;

public class GetLowStockItemsQueryHandler : IRequestHandler<GetLowStockItemsQuery, List<LowStockItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLowStockItemsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LowStockItemDto>> Handle(GetLowStockItemsQuery request, CancellationToken cancellationToken)
    {
        var lowStockItems = await _context.Set<InventoryEntity>()
            .Include(i => i.Item)
            .Where(i => i.QuantityAvailable <= i.ReorderLevel)
            .ToListAsync(cancellationToken);

        return lowStockItems.Select(i => new LowStockItemDto
        {
            ItemId = i.ItemId,
            ItemNameEn = i.Item.NameEn,
            ItemSku = i.Item.Sku,
            QuantityOnHand = i.QuantityOnHand,
            QuantityReserved = i.QuantityReserved,
            QuantityAvailable = i.QuantityAvailable,
            ReorderLevel = i.ReorderLevel,
            IsBelowReorderLevel = i.QuantityAvailable < i.ReorderLevel
        }).ToList();
    }
}

