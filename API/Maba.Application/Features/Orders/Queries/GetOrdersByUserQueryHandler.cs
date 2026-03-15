using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Orders.Queries;
using Maba.Application.Features.Orders.DTOs;

namespace Maba.Application.Features.Orders.Handlers;

public class GetOrdersByUserQueryHandler : IRequestHandler<GetOrdersByUserQuery, List<OrderDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOrdersByUserQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrderDto>> Handle(GetOrdersByUserQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Domain.Orders.Order>()
            .Include(o => o.User)
            .Include(o => o.OrderStatus)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Item)
            .Where(o => o.UserId == request.UserId);

        if (request.OrderStatusId.HasValue)
        {
            query = query.Where(o => o.OrderStatusId == request.OrderStatusId.Value);
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(order => new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            UserFullName = order.User.FullName,
            OrderStatusId = order.OrderStatusId,
            OrderStatusKey = order.OrderStatus.Key,
            SubTotal = order.SubTotal,
            TaxAmount = order.TaxAmount,
            ShippingCost = order.ShippingCost,
            DiscountAmount = order.DiscountAmount,
            Total = order.Total,
            Currency = order.Currency,
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
                ItemNameEn = oi.Item?.NameEn,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                DiscountAmount = oi.DiscountAmount,
                TaxAmount = oi.TaxAmount,
                LineTotal = oi.LineTotal,
                MetaJson = oi.MetaJson,
                CreatedAt = oi.CreatedAt
            }).ToList(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        }).ToList();
    }
}



