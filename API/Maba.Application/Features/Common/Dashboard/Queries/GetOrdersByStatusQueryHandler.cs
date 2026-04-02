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
        IQueryable<Order> query = _context.Set<Order>()
            .AsNoTracking()
            .Include(o => o.OrderStatus);

        if (request.FromDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= request.ToDate.Value);
        }

        var orders = await query.ToListAsync(cancellationToken);

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
