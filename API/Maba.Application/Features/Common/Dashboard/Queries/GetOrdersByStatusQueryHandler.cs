using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Common.Dashboard.DTOs;
using Maba.Application.Features.Common.Dashboard.Queries;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Common.Dashboard.Handlers;

public class GetOrdersByStatusQueryHandler : IRequestHandler<GetOrdersByStatusQuery, List<OrdersByStatusDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOrdersByStatusQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrdersByStatusDto>> Handle(GetOrdersByStatusQuery request, CancellationToken cancellationToken)
    {
        var orders = await _context.Set<Order>()
            .Include(o => o.OrderStatus)
            .ToListAsync(cancellationToken);

        var result = orders
            .GroupBy(o => o.OrderStatus?.NameEn ?? "Unknown")
            .Select(g => new OrdersByStatusDto
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToList();

        return result;
    }
}
