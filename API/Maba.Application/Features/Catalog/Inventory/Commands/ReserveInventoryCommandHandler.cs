using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Inventory.Commands;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Domain.Catalog;
using InventoryEntity = Maba.Domain.Catalog.Inventory;

namespace Maba.Application.Features.Catalog.Inventory.Handlers;

public class ReserveInventoryCommandHandler : IRequestHandler<ReserveInventoryCommand, InventoryDto>
{
    private readonly IApplicationDbContext _context;

    public ReserveInventoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryDto> Handle(ReserveInventoryCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _context.Set<InventoryEntity>()
            .FirstOrDefaultAsync(i => i.ItemId == request.ItemId, cancellationToken);

        if (inventory == null)
        {
            throw new KeyNotFoundException("Inventory not found for this item.");
        }

        // Check available quantity
        if (inventory.QuantityAvailable < request.Quantity)
        {
            throw new InvalidOperationException($"Insufficient inventory. Available: {inventory.QuantityAvailable}, Requested: {request.Quantity}");
        }

        // Reserve inventory
        inventory.QuantityReserved += request.Quantity;
        inventory.UpdatedAt = DateTime.UtcNow;

        // Create transaction record
        var transaction = new InventoryTransaction
        {
            Id = Guid.NewGuid(),
            InventoryId = inventory.Id,
            TransactionType = "Reservation",
            Quantity = request.Quantity,
            Reason = "Order Reservation",
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

