using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Inventory.Queries;
using Maba.Application.Features.Catalog.Items.DTOs;
using DomainInventory = Maba.Domain.Catalog.Inventory;

namespace Maba.Application.Features.Catalog.Inventory.Handlers;

public class GetInventoryByItemIdQueryHandler : IRequestHandler<GetInventoryByItemIdQuery, InventoryDto?>
{
    private readonly IApplicationDbContext _context;

    public GetInventoryByItemIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryDto?> Handle(GetInventoryByItemIdQuery request, CancellationToken cancellationToken)
    {
        var inventory = await _context.Set<DomainInventory>()
            .FirstOrDefaultAsync(i => i.ItemId == request.ItemId, cancellationToken);

        if (inventory == null)
        {
            return null;
        }

        return new InventoryDto
        {
            Id = inventory.Id,
            ItemId = inventory.ItemId,
            QuantityOnHand = inventory.QuantityOnHand,
            ReorderLevel = inventory.ReorderLevel,
            LastStockInAt = inventory.LastStockInAt
        };
    }
}

