using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Common.Models;
using Maba.Application.Features.Orders.DTOs;
using Maba.Application.Features.Orders.Queries;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Orders.Handlers;

public class GetOrdersPagedQueryHandler : IRequestHandler<GetOrdersPagedQuery, PagedResult<OrderDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOrdersPagedQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetOrdersPagedQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Order>()
            .Include(o => o.User)
            .Include(o => o.OrderStatus)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Item)
            .AsQueryable();

        if (request.UserId.HasValue)
        {
            query = query.Where(o => o.UserId == request.UserId.Value);
        }

        if (request.OrderStatusId.HasValue)
        {
            query = query.Where(o => o.OrderStatusId == request.OrderStatusId.Value);
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= request.DateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(o => 
                o.OrderNumber.ToLower().Contains(searchTerm) ||
                o.User.FullName.ToLower().Contains(searchTerm));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            query = request.SortBy.ToLower() switch
            {
                "ordernumber" => request.SortDescending 
                    ? query.OrderByDescending(o => o.OrderNumber)
                    : query.OrderBy(o => o.OrderNumber),
                "createdat" => request.SortDescending
                    ? query.OrderByDescending(o => o.CreatedAt)
                    : query.OrderBy(o => o.CreatedAt),
                "total" => request.SortDescending
                    ? query.OrderByDescending(o => o.Total)
                    : query.OrderBy(o => o.Total),
                _ => query.OrderByDescending(o => o.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(o => o.CreatedAt);
        }

        // Apply pagination
        var orders = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = orders.Select(o => new OrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            UserId = o.UserId,
            UserFullName = o.User.FullName,
            OrderStatusId = o.OrderStatusId,
            OrderStatusKey = o.OrderStatus.Key,
            SubTotal = o.SubTotal,
            TaxAmount = o.TaxAmount,
            ShippingCost = o.ShippingCost,
            DiscountAmount = o.DiscountAmount,
            Total = o.Total,
            Currency = o.Currency,
            ShippingAddressJson = o.ShippingAddress,
            BillingAddressJson = o.BillingAddress,
            Notes = o.Notes,
            EstimatedDeliveryDate = o.EstimatedDeliveryDate,
            TrackingNumber = o.TrackingNumber,
            OrderItems = o.OrderItems.Select(oi => new OrderItemDto
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
            CreatedAt = o.CreatedAt,
            UpdatedAt = o.UpdatedAt
        }).ToList();

        return new PagedResult<OrderDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}




