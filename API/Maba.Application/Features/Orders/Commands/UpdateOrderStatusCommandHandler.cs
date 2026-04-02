using System.Security.Claims;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Maba.Application.Common.Emails;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Orders.Commands;
using Maba.Application.Features.Orders.DTOs;
using Maba.Domain.Orders;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Orders.Handlers;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, OrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly IAuditService _auditService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateOrderStatusCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        IConfiguration configuration,
        IAuditService auditService,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
        _auditService = auditService;
        _httpContextAccessor = httpContextAccessor;
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

        Guid? actorUserId = null;
        var sub = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(sub, out var parsedUserId))
        {
            actorUserId = parsedUserId;
        }

        var oldAudit = JsonSerializer.Serialize(new { orderStatusId = order.OrderStatusId, orderStatusKey = oldStatusKey });
        var newAudit = JsonSerializer.Serialize(new { orderStatusId = request.OrderStatusId, orderStatusKey = newStatusKey });
        await _auditService.LogActionAsync(
            "Order",
            order.Id,
            "UpdateStatus",
            actorUserId,
            oldAudit,
            newAudit,
            cancellationToken: cancellationToken);

        var shouldSendShippedEmail = newStatusKey == "Shipped" && oldStatusKey != "Shipped";

        // Reload order with updated status
        var updatedOrder = await _context.Set<Order>()
            .Include(o => o.User)
            .Include(o => o.OrderStatus)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Item)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == order.Id, cancellationToken);

        if (shouldSendShippedEmail && updatedOrder?.User != null)
        {
            var frontendBase = _configuration["App:FrontendBaseUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
            var tracking = updatedOrder.TrackingNumber?.Trim();
            string? trackingUrl = null;
            if (!string.IsNullOrEmpty(tracking) &&
                (tracking.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                 tracking.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
            {
                trackingUrl = tracking;
            }

            DateTime? estUtc = null;
            if (updatedOrder.EstimatedDeliveryDate.HasValue)
            {
                var d = updatedOrder.EstimatedDeliveryDate.Value;
                estUtc = d.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(d, DateTimeKind.Utc)
                    : d.ToUniversalTime();
            }

            var shippedModel = new ShopOrderShippedEmailModel
            {
                OrderNumber = updatedOrder.OrderNumber,
                TrackingNumber = tracking,
                Carrier = null,
                ShippedDateUtc = DateTime.UtcNow,
                EstimatedDeliveryUtc = estUtc,
                ViewOrderUrl = $"{frontendBase}/account/orders/{updatedOrder.Id}",
                TrackingUrl = trackingUrl,
                PublicSiteUrl = frontendBase
            };
            await _emailService.SendShopOrderShippedAsync(updatedOrder.User.Email, shippedModel, cancellationToken);
        }

        var totalPaid = updatedOrder!.Payments?.Sum(p => p.Amount) ?? 0;
        var paymentStatus = "Pending";
        if (totalPaid >= updatedOrder.Total) paymentStatus = "Paid";
        else if (totalPaid > 0) paymentStatus = "PartiallyPaid";

        return new OrderDto
        {
            Id = updatedOrder.Id,
            OrderNumber = updatedOrder.OrderNumber,
            UserId = updatedOrder.UserId,
            UserFullName = updatedOrder.User.FullName,
            OrderStatusId = updatedOrder.OrderStatusId,
            OrderStatusKey = updatedOrder.OrderStatus.Key,
            Status = updatedOrder.OrderStatus != null
                ? new OrderStatusDto
                {
                    Id = updatedOrder.OrderStatus.Id,
                    Key = updatedOrder.OrderStatus.Key,
                    NameEn = updatedOrder.OrderStatus.NameEn,
                    NameAr = updatedOrder.OrderStatus.NameAr
                }
                : null,
            SubTotal = updatedOrder.SubTotal,
            TaxAmount = updatedOrder.TaxAmount,
            ShippingCost = updatedOrder.ShippingCost,
            DiscountAmount = updatedOrder.DiscountAmount,
            Total = updatedOrder.Total,
            Currency = updatedOrder.Currency,
            PaymentStatus = paymentStatus,
            TrackingNumber = updatedOrder.TrackingNumber,
            EstimatedDeliveryDate = updatedOrder.EstimatedDeliveryDate,
            OrderItems = updatedOrder.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                OrderId = oi.OrderId,
                ItemId = oi.ItemId,
                ItemNameEn = oi.Item?.NameEn,
                ItemNameAr = oi.Item?.NameAr,
                ItemSku = oi.Item?.Sku,
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

