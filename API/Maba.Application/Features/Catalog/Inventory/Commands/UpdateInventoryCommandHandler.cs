using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Inventory.Commands;
using Maba.Application.Features.Catalog.Items.DTOs;
using DomainInventory = Maba.Domain.Catalog.Inventory;

namespace Maba.Application.Features.Catalog.Inventory.Handlers;

public class UpdateInventoryCommandHandler : IRequestHandler<UpdateInventoryCommand, InventoryDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateInventoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryDto> Handle(UpdateInventoryCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _context.Set<DomainInventory>()
            .FirstOrDefaultAsync(i => i.ItemId == request.ItemId, cancellationToken);

        if (inventory == null)
        {
            // Create new inventory
            inventory = new DomainInventory
            {
                Id = Guid.NewGuid(),
                ItemId = request.ItemId,
                QuantityOnHand = request.QuantityOnHand,
                ReorderLevel = request.ReorderLevel,
                LastStockInAt = DateTime.UtcNow
            };
            _context.Set<DomainInventory>().Add(inventory);
        }
        else
        {
            var oldQuantity = inventory.QuantityOnHand;
            inventory.QuantityOnHand = request.QuantityOnHand;
            inventory.ReorderLevel = request.ReorderLevel;
            
            // Update LastStockInAt if quantity increased
            if (request.QuantityOnHand > oldQuantity)
            {
                inventory.LastStockInAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

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

