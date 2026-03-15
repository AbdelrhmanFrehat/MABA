using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Orders.Queries;
using Maba.Application.Features.Orders.DTOs;
using Maba.Domain.Media;

namespace Maba.Application.Features.Orders.Handlers;

public class GetOrderDetailQueryHandler : IRequestHandler<GetOrderDetailQuery, OrderDto>
{
    private readonly IApplicationDbContext _context;

    public GetOrderDetailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Set<Domain.Orders.Order>()
            .Include(o => o.User)
            .Include(o => o.OrderStatus)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
            .Include(o => o.Invoices)
            .Include(o => o.Payments)
            .Include(o => o.PaymentPlan)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found.");
        }

        // Get primary images for items
        var itemIds = order.OrderItems.Select(oi => oi.ItemId).Where(id => id.HasValue).Select(id => id!.Value).ToList();
        var primaryImages = new Dictionary<Guid, string>();
        
        if (itemIds.Any())
        {
            primaryImages = await _context.Set<EntityMediaLink>()
                .Include(eml => eml.MediaAsset)
                .Where(eml => eml.EntityType == "Item" && itemIds.Contains(eml.EntityId) && eml.IsPrimary)
                .ToDictionaryAsync(eml => eml.EntityId, eml => eml.MediaAsset.FileUrl, cancellationToken);
        }

        // Determine payment status
        var totalPaid = order.Payments?.Sum(p => p.Amount) ?? 0;
        string paymentStatus = "Pending";
        if (totalPaid >= order.Total) paymentStatus = "Paid";
        else if (totalPaid > 0) paymentStatus = "PartiallyPaid";

        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            UserFullName = order.User?.FullName ?? "",
            CustomerName = order.User?.FullName,
            CustomerEmail = order.User?.Email,
            OrderStatusId = order.OrderStatusId,
            OrderStatusKey = order.OrderStatus?.Key ?? "",
            Status = order.OrderStatus != null ? new OrderStatusDto
            {
                Id = order.OrderStatus.Id,
                Key = order.OrderStatus.Key,
                NameEn = order.OrderStatus.NameEn,
                NameAr = order.OrderStatus.NameAr
            } : null,
            SubTotal = order.SubTotal,
            TaxAmount = order.TaxAmount,
            ShippingCost = order.ShippingCost,
            DiscountAmount = order.DiscountAmount,
            Total = order.Total,
            Currency = order.Currency,
            PaymentStatus = paymentStatus,
            ShippingAddressJson = order.ShippingAddress,
            BillingAddressJson = order.BillingAddress,
            Notes = order.Notes,
            EstimatedDeliveryDate = order.EstimatedDeliveryDate,
            TrackingNumber = order.TrackingNumber,
            OrderItems = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                OrderId = oi.OrderId,
                ItemId = oi.ItemId,
                ItemNameEn = oi.Item?.NameEn ?? "",
                ItemNameAr = oi.Item?.NameAr ?? "",
                ItemSku = oi.Item?.Sku ?? "",
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                DiscountAmount = oi.DiscountAmount,
                TaxAmount = oi.TaxAmount,
                LineTotal = oi.LineTotal,
                MediaAssetUrl = oi.ItemId.HasValue && primaryImages.ContainsKey(oi.ItemId.Value) 
                    ? primaryImages[oi.ItemId.Value] 
                    : null,
                MetaJson = oi.MetaJson,
                CreatedAt = oi.CreatedAt
            }).ToList(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }
}
