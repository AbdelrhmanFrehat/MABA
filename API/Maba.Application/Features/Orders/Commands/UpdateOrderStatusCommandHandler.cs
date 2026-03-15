using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Orders.Commands;
using Maba.Application.Features.Orders.DTOs;
using Maba.Domain.Orders;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Orders.Handlers;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, OrderDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateOrderStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Set<Order>()
            .Include(o => o.User)
            .Include(o => o.OrderStatus)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Item)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found.");
        }

        var newStatus = await _context.Set<OrderStatus>()
            .FirstOrDefaultAsync(s => s.Id == request.OrderStatusId, cancellationToken);

        if (newStatus == null)
        {
            throw new KeyNotFoundException("Order status not found.");
        }

        // Validate status transition
        var oldStatusKey = order.OrderStatus.Key;
        var newStatusKey = newStatus.Key;

        // Business rules for status transitions
        if (oldStatusKey == "Cancelled" && newStatusKey != "Cancelled")
        {
            throw new InvalidOperationException("Cannot change status of a cancelled order.");
        }

        if (oldStatusKey == "Delivered" && newStatusKey != "Delivered")
        {
            throw new InvalidOperationException("Cannot change status of a delivered order.");
        }

        // Update status
        order.OrderStatusId = request.OrderStatusId;
        if (!string.IsNullOrEmpty(request.TrackingNumber))
        {
            order.TrackingNumber = request.TrackingNumber;
        }
        if (request.EstimatedDeliveryDate.HasValue)
        {
            order.EstimatedDeliveryDate = request.EstimatedDeliveryDate;
        }
        order.UpdatedAt = DateTime.UtcNow;

        // If status changed to "Shipped" or "Delivered", deduct inventory
        if ((newStatusKey == "Shipped" || newStatusKey == "Delivered") && 
            (oldStatusKey != "Shipped" && oldStatusKey != "Delivered"))
        {
            foreach (var orderItem in order.OrderItems)
            {
                var inventory = await _context.Set<Inventory>()
                    .FirstOrDefaultAsync(i => i.ItemId == orderItem.ItemId, cancellationToken);

                if (inventory != null)
                {
                    // Deduct from reserved and on-hand
                    inventory.QuantityReserved = Math.Max(0, inventory.QuantityReserved - orderItem.Quantity);
                    inventory.QuantityOnHand = Math.Max(0, inventory.QuantityOnHand - orderItem.Quantity);
                    inventory.LastStockOutAt = DateTime.UtcNow;
                    inventory.UpdatedAt = DateTime.UtcNow;

                    // Create transaction
                    var transaction = new InventoryTransaction
                    {
                        Id = Guid.NewGuid(),
                        InventoryId = inventory.Id,
                        TransactionType = "StockOut",
                        Quantity = orderItem.Quantity,
                        Reason = $"Order {order.OrderNumber} - {newStatusKey}",
                        OrderId = order.Id
                    };

                    _context.Set<InventoryTransaction>().Add(transaction);
                }
            }
        }

        // If status changed to "Cancelled", restore inventory
        if (newStatusKey == "Cancelled" && oldStatusKey != "Cancelled")
        {
            foreach (var orderItem in order.OrderItems)
            {
                var inventory = await _context.Set<Inventory>()
                    .FirstOrDefaultAsync(i => i.ItemId == orderItem.ItemId, cancellationToken);

                if (inventory != null)
                {
                    // Release reserved inventory
                    inventory.QuantityReserved = Math.Max(0, inventory.QuantityReserved - orderItem.Quantity);
                    inventory.UpdatedAt = DateTime.UtcNow;

                    // Create transaction
                    var transaction = new InventoryTransaction
                    {
                        Id = Guid.NewGuid(),
                        InventoryId = inventory.Id,
                        TransactionType = "Release",
                        Quantity = orderItem.Quantity,
                        Reason = $"Order {order.OrderNumber} - Cancelled",
                        OrderId = order.Id
                    };

                    _context.Set<InventoryTransaction>().Add(transaction);
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Reload order with updated status
        var updatedOrder = await _context.Set<Order>()
            .Include(o => o.User)
            .Include(o => o.OrderStatus)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Item)
            .FirstOrDefaultAsync(o => o.Id == order.Id, cancellationToken);

        return new OrderDto
        {
            Id = updatedOrder!.Id,
            OrderNumber = updatedOrder.OrderNumber,
            UserId = updatedOrder.UserId,
            UserFullName = updatedOrder.User.FullName,
            OrderStatusId = updatedOrder.OrderStatusId,
            OrderStatusKey = updatedOrder.OrderStatus.Key,
            SubTotal = updatedOrder.SubTotal,
            TaxAmount = updatedOrder.TaxAmount,
            ShippingCost = updatedOrder.ShippingCost,
            DiscountAmount = updatedOrder.DiscountAmount,
            Total = updatedOrder.Total,
            Currency = updatedOrder.Currency,
            TrackingNumber = updatedOrder.TrackingNumber,
            EstimatedDeliveryDate = updatedOrder.EstimatedDeliveryDate,
            OrderItems = updatedOrder.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                OrderId = oi.OrderId,
                ItemId = oi.ItemId,
                ItemNameEn = oi.Item?.NameEn,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                DiscountAmount = oi.DiscountAmount,
                TaxAmount = oi.TaxAmount,
                LineTotal = oi.LineTotal,
                MetaJson = oi.MetaJson,
                CreatedAt = oi.CreatedAt
            }).ToList(),
            CreatedAt = updatedOrder.CreatedAt,
            UpdatedAt = updatedOrder.UpdatedAt
        };
    }
}

