using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Inventory.Commands;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Domain.Catalog;
using InventoryEntity = Maba.Domain.Catalog.Inventory;

namespace Maba.Application.Features.Catalog.Inventory.Handlers;

public class AdjustInventoryCommandHandler : IRequestHandler<AdjustInventoryCommand, InventoryDto>
{
    private readonly IApplicationDbContext _context;

    public AdjustInventoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryDto> Handle(AdjustInventoryCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _context.Set<InventoryEntity>()
            .FirstOrDefaultAsync(i => i.ItemId == request.ItemId, cancellationToken);

        if (inventory == null)
        {
            throw new KeyNotFoundException("Inventory not found for this item.");
        }

        var newQuantity = inventory.QuantityOnHand + request.QuantityChange;

        // Prevent negative inventory
        if (newQuantity < 0)
        {
            throw new InvalidOperationException("Cannot adjust inventory to negative quantity.");
        }

        // Update inventory
        inventory.QuantityOnHand = newQuantity;
        if (request.CostPerUnit.HasValue)
        {
            inventory.CostPerUnit = request.CostPerUnit.Value;
        }

        if (request.QuantityChange > 0)
        {
            inventory.LastStockInAt = DateTime.UtcNow;
        }
        else if (request.QuantityChange < 0)
        {
            inventory.LastStockOutAt = DateTime.UtcNow;
        }

        inventory.UpdatedAt = DateTime.UtcNow;

        // Create transaction record
        var transaction = new InventoryTransaction
        {
            Id = Guid.NewGuid(),
            InventoryId = inventory.Id,
            TransactionType = request.QuantityChange > 0 ? "StockIn" : "Adjustment",
            Quantity = Math.Abs(request.QuantityChange),
            CostPerUnit = request.CostPerUnit,
            Reason = request.Reason,
            Notes = request.Notes,
            CreatedByUserId = request.CreatedByUserId
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

