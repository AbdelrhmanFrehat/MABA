using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Maba.Application.Common.Emails;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Orders.Commands;
using Maba.Application.Features.Orders.DTOs;
using Maba.Domain.Orders;
using Maba.Domain.Catalog;
using Maba.Domain.Users;

namespace Maba.Application.Features.Orders.Handlers;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public CreateOrderCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Set<Domain.Users.User>()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        var pendingStatus = await _context.Set<OrderStatus>()
            .FirstOrDefaultAsync(s => s.Key == "Pending", cancellationToken);

        if (pendingStatus == null)
        {
            throw new KeyNotFoundException("Pending order status not found");
        }

        // Generate order number
        var orderNumber = await GenerateOrderNumberAsync(cancellationToken);

        decimal subTotal = 0;
        var orderItems = new List<OrderItem>();

        foreach (var itemInput in request.OrderItems)
        {
            var item = await _context.Set<Item>()
                .Include(i => i.Inventory)
                .FirstOrDefaultAsync(i => i.Id == itemInput.ItemId, cancellationToken);

            if (item == null)
            {
                throw new KeyNotFoundException($"Item {itemInput.ItemId} not found");
            }

            // Check inventory availability
            if (item.Inventory == null)
            {
                throw new InvalidOperationException($"Item {item.NameEn} has no inventory record.");
            }

            if (item.Inventory.QuantityAvailable < itemInput.Quantity)
            {
                throw new InvalidOperationException($"Insufficient inventory for item {item.NameEn}. Available: {item.Inventory.QuantityAvailable}, Requested: {itemInput.Quantity}");
            }

            var lineSubTotal = itemInput.UnitPrice * itemInput.Quantity;
            var lineDiscount = 0m; // TODO: Calculate discount if applicable
            var lineTax = 0m; // TODO: Calculate tax if applicable
            var lineTotal = lineSubTotal - lineDiscount + lineTax;
            subTotal += lineSubTotal;

            orderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.Empty, // Will be set after order creation
                ItemId = itemInput.ItemId,
                Quantity = itemInput.Quantity,
                UnitPrice = itemInput.UnitPrice,
                DiscountAmount = lineDiscount,
                TaxAmount = lineTax,
                LineTotal = lineTotal,
                MetaJson = itemInput.MetaJson
            });
        }

        // Calculate totals
        var taxAmount = 0m; // TODO: Calculate tax based on tax rate
        var shippingCost = 0m; // TODO: Calculate shipping based on method
        var discountAmount = 0m; // TODO: Calculate discount if applicable
        var total = subTotal + taxAmount + shippingCost - discountAmount;

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
            ShippingAddress = request.ShippingAddress,
            BillingAddress = request.BillingAddress
        };

        _context.Set<Order>().Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        // Set order ID for order items and reserve inventory
        foreach (var orderItem in orderItems)
        {
            orderItem.OrderId = order.Id;
            _context.Set<OrderItem>().Add(orderItem);

            // Reserve inventory for this order item
            var inventory = await _context.Set<Inventory>()
                .FirstOrDefaultAsync(i => i.ItemId == orderItem.ItemId, cancellationToken);

            if (inventory != null)
            {
                // Reserve inventory
                inventory.QuantityReserved += orderItem.Quantity;
                inventory.UpdatedAt = DateTime.UtcNow;

                // Create transaction record
                var transaction = new InventoryTransaction
                {
                    Id = Guid.NewGuid(),
                    InventoryId = inventory.Id,
                    TransactionType = "Reservation",
                    Quantity = orderItem.Quantity,
                    Reason = "Order Reservation",
                    OrderId = order.Id
                };

                _context.Set<InventoryTransaction>().Add(transaction);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Load order with relations
        var orderWithRelations = await _context.Set<Order>()
            .Include(o => o.User)
            .Include(o => o.OrderStatus)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Item)
            .FirstOrDefaultAsync(o => o.Id == order.Id, cancellationToken);

        var frontendBase = _configuration["App:FrontendBaseUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
        if (orderWithRelations?.User != null)
        {
            var shipAddr = AddressDto.FromJson(orderWithRelations.ShippingAddress);
            var billAddr = AddressDto.FromJson(orderWithRelations.BillingAddress);
            var confirmModel = new ShopOrderConfirmationEmailModel
            {
                OrderNumber = orderWithRelations.OrderNumber,
                OrderDateUtc = orderWithRelations.CreatedAt,
                CustomerName = !string.IsNullOrWhiteSpace(shipAddr?.FullName) ? shipAddr!.FullName.Trim() : orderWithRelations.User.FullName,
                PaymentMethod = null,
                ShippingMethod = null,
                Items = orderWithRelations.OrderItems.Select(oi => new ShopOrderEmailLineItem
                {
                    ProductName = oi.Item?.NameEn?.Trim() ?? "Product",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    LineTotal = oi.UnitPrice * oi.Quantity
                }).ToList(),
                SubTotal = orderWithRelations.SubTotal,
                Shipping = orderWithRelations.ShippingCost,
                Tax = orderWithRelations.TaxAmount,
                Discount = orderWithRelations.DiscountAmount,
                Total = orderWithRelations.Total,
                Currency = orderWithRelations.Currency,
                ShippingAddressLinesHtml = ShopOrderEmailHtmlBuilder.FormatAddressLines(shipAddr),
                BillingAddressLinesHtml = ShopOrderEmailHtmlBuilder.FormatAddressLines(billAddr),
                ViewOrderUrl = $"{frontendBase}/account/orders/{orderWithRelations.Id}",
                PublicSiteUrl = frontendBase
            };
            await _emailService.SendShopOrderConfirmationAsync(orderWithRelations.User.Email, confirmModel, cancellationToken);
        }

        return new OrderDto
        {
            Id = orderWithRelations!.Id,
            UserId = orderWithRelations.UserId,
            UserFullName = orderWithRelations.User.FullName,
            OrderNumber = orderWithRelations.OrderNumber,
            OrderStatusId = orderWithRelations.OrderStatusId,
            OrderStatusKey = orderWithRelations.OrderStatus.Key,
            SubTotal = orderWithRelations.SubTotal,
            TaxAmount = orderWithRelations.TaxAmount,
            ShippingCost = orderWithRelations.ShippingCost,
            DiscountAmount = orderWithRelations.DiscountAmount,
            Total = orderWithRelations.Total,
            Currency = orderWithRelations.Currency,
            ShippingAddressJson = orderWithRelations.ShippingAddress,
            BillingAddressJson = orderWithRelations.BillingAddress,
            Notes = orderWithRelations.Notes,
            EstimatedDeliveryDate = orderWithRelations.EstimatedDeliveryDate,
            TrackingNumber = orderWithRelations.TrackingNumber,
            OrderItems = orderWithRelations.OrderItems.Select(oi => new OrderItemDto
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
            CreatedAt = orderWithRelations.CreatedAt,
            UpdatedAt = orderWithRelations.UpdatedAt
        };
    }

    private async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"ORD-{year}-";
        
        // Get the last order number for this year
        var lastOrder = await _context.Set<Order>()
            .Where(o => o.OrderNumber.StartsWith(prefix))
            .OrderByDescending(o => o.OrderNumber)
            .FirstOrDefaultAsync(cancellationToken);

        int sequence = 1;
        if (lastOrder != null)
        {
            var lastSequence = lastOrder.OrderNumber.Replace(prefix, "");
            if (int.TryParse(lastSequence, out int lastSeq))
            {
                sequence = lastSeq + 1;
            }
        }

        return $"{prefix}{sequence:D6}";
    }
}



