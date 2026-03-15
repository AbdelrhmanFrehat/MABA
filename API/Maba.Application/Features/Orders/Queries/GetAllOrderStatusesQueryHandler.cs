using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Orders.DTOs;
using Maba.Application.Features.Orders.Queries;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Orders.Handlers;

public class GetAllOrderStatusesQueryHandler : IRequestHandler<GetAllOrderStatusesQuery, List<OrderStatusDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllOrderStatusesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrderStatusDto>> Handle(GetAllOrderStatusesQuery request, CancellationToken cancellationToken)
    {
        var statuses = await _context.Set<OrderStatus>()
            .OrderBy(s => s.NameEn)
            .ToListAsync(cancellationToken);

        return statuses.Select(s => new OrderStatusDto
        {
            Id = s.Id,
            Key = s.Key,
            NameEn = s.NameEn,
            NameAr = s.NameAr
        }).ToList();
    }
}
