using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Orders.DTOs;
using Maba.Domain.Orders;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Cart.Commands;

public class CheckoutCartCommandHandler : IRequestHandler<CheckoutCartCommand, OrderDto>
{
    private readonly IApplicationDbContext _context;

    public CheckoutCartCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto> Handle(CheckoutCartCommand request, CancellationToken cancellationToken)
    {
        // Get cart
        Domain.Orders.Cart? cart = null;

        if (request.UserId != Guid.Empty)
        {
            cart = await _context.Set<Domain.Orders.Cart>()
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Item)
                        .ThenInclude(i => i.Inventory)
                .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(request.SessionId))
        {
            cart = await _context.Set<Domain.Orders.Cart>()
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Item)
                        .ThenInclude(i => i.Inventory)
                .FirstOrDefaultAsync(c => c.SessionId == request.SessionId, cancellationToken);
        }

        if (cart == null || !cart.Items.Any())
        {
            throw new InvalidOperationException("Cart is empty or not found");
        }

        // Validate inventory for all items
        foreach (var cartItem in cart.Items)
        {
            if (cartItem.Item.Inventory == null)
            {
                throw new InvalidOperationException($"Item {cartItem.Item.NameEn} has no inventory record");
            }

            if (cartItem.Item.Inventory.QuantityAvailable < cartItem.Quantity)
            {
                throw new InvalidOperationException($"Insufficient inventory for {cartItem.Item.NameEn}. Available: {cartItem.Item.Inventory.QuantityAvailable}");
            }
        }

        // Get pending status
        var pendingStatus = await _context.Set<OrderStatus>()
            .FirstOrDefaultAsync(s => s.Key == "Pending", cancellationToken)
            ?? throw new KeyNotFoundException("Pending order status not found");

        // Calculate totals
        decimal subTotal = cart.Items.Sum(ci => ci.UnitPrice * ci.Quantity);
        decimal taxAmount = subTotal * 0.15m; // 15% VAT
        decimal shippingCost = subTotal > 200 ? 0 : 25m; // Free shipping over 200
        decimal discountAmount = cart.CouponDiscount;
        decimal total = subTotal + taxAmount + shippingCost - discountAmount;

        // Generate order number
        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        // Ensure we have a user for the order
        if (request.UserId == Guid.Empty)
        {
            throw new InvalidOperationException("User authentication is required for checkout");
        }

        // Create order
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            UserId = request.UserId,
            OrderStatusId = pendingStatus.Id,
            SubTotal = subTotal,
            TaxAmount = taxAmount,
            ShippingCost = shippingCost,
            DiscountAmount = discountAmount,
            Total = total,
            Currency = "ILS",
            ShippingAddress = request.ShippingAddressJson,
            BillingAddress = request.BillingAddressJson,
            Notes = request.Notes
        };

        _context.Set<Order>().Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        // Create order items and reserve inventory
        foreach (var cartItem in cart.Items)
        {
            var lineTotal = cartItem.UnitPrice * cartItem.Quantity;
            
            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ItemId = cartItem.ItemId,
                Quantity = cartItem.Quantity,
                UnitPrice = cartItem.UnitPrice,
                DiscountAmount = 0,
                TaxAmount = lineTotal * 0.15m,
                LineTotal = lineTotal + (lineTotal * 0.15m)
            };
            _context.Set<OrderItem>().Add(orderItem);

            // Reserve inventory
            var inventory = cartItem.Item.Inventory!;
            inventory.QuantityReserved += cartItem.Quantity;
            inventory.UpdatedAt = DateTime.UtcNow;

            // Create transaction
            var transaction = new InventoryTransaction
            {
                Id = Guid.NewGuid(),
                InventoryId = inventory.Id,
                TransactionType = "Reservation",
                Quantity = cartItem.Quantity,
                Reason = $"Order {orderNumber}",
                OrderId = order.Id
            };
            _context.Set<InventoryTransaction>().Add(transaction);
        }

        // Clear cart
        _context.Set<CartItem>().RemoveRange(cart.Items);
        _context.Set<Domain.Orders.Cart>().Remove(cart);

        await _context.SaveChangesAsync(cancellationToken);

        // Return order DTO
        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderNumber = order.OrderNumber,
            OrderStatusId = order.OrderStatusId,
            OrderStatusKey = pendingStatus.Key,
            Status = new OrderStatusDto
            {
                Id = pendingStatus.Id,
                Key = pendingStatus.Key,
                NameEn = pendingStatus.NameEn,
                NameAr = pendingStatus.NameAr
            },
            SubTotal = order.SubTotal,
            TaxAmount = order.TaxAmount,
            ShippingCost = order.ShippingCost,
            DiscountAmount = order.DiscountAmount,
            Total = order.Total,
            Currency = order.Currency,
            PaymentStatus = "Pending",
            ShippingAddressJson = order.ShippingAddress,
            BillingAddressJson = order.BillingAddress,
            Notes = order.Notes,
            OrderItems = cart.Items.Select(ci => new OrderItemDto
            {
                ItemId = ci.ItemId,
                ItemNameEn = ci.Item?.NameEn ?? "",
                ItemNameAr = ci.Item?.NameAr ?? "",
                ItemSku = ci.Item?.Sku ?? "",
                Quantity = ci.Quantity,
                UnitPrice = ci.UnitPrice,
                TaxAmount = ci.UnitPrice * ci.Quantity * 0.15m,
                LineTotal = ci.UnitPrice * ci.Quantity * 1.15m
            }).ToList(),
            CreatedAt = order.CreatedAt
        };
    }
}
