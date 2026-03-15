using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Inventory.Commands;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Domain.Catalog;
using InventoryEntity = Maba.Domain.Catalog.Inventory;

namespace Maba.Application.Features.Catalog.Inventory.Handlers;

public class ReleaseInventoryCommandHandler : IRequestHandler<ReleaseInventoryCommand, InventoryDto>
{
    private readonly IApplicationDbContext _context;

    public ReleaseInventoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryDto> Handle(ReleaseInventoryCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _context.Set<InventoryEntity>()
            .FirstOrDefaultAsync(i => i.ItemId == request.ItemId, cancellationToken);

        if (inventory == null)
        {
            throw new KeyNotFoundException("Inventory not found for this item.");
        }

        // Release reserved inventory
        if (inventory.QuantityReserved < request.Quantity)
        {
            throw new InvalidOperationException($"Cannot release more than reserved. Reserved: {inventory.QuantityReserved}, Requested: {request.Quantity}");
        }

        inventory.QuantityReserved -= request.Quantity;
        inventory.UpdatedAt = DateTime.UtcNow;

        // Create transaction record
        var transaction = new InventoryTransaction
        {
            Id = Guid.NewGuid(),
            InventoryId = inventory.Id,
            TransactionType = "Release",
            Quantity = request.Quantity,
            Reason = "Order Cancellation",
            OrderId = request.OrderId
        };

        _context.Set<InventoryTransaction>().Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return new InventoryDto
        {
            Id = inventory.Id,
            ItemId = inventory.ItemId,
            QuantityOnHand = inventory.QuantityOnHand,
            QuantityReserved = inventory.QuantityReserved,
            QuantityAvailable = inventory.QuantityAvailable,
            QuantityOnOrder = inventory.QuantityOnOrder,
            ReorderLevel = inventory.ReorderLevel,
            CostPerUnit = inventory.CostPerUnit,
            LastStockInAt = inventory.LastStockInAt
        };
    }
}

