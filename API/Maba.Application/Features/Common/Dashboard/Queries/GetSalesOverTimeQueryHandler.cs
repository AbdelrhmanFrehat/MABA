using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Common.Dashboard.DTOs;
using Maba.Application.Features.Common.Dashboard.Queries;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Common.Dashboard.Handlers;

public class GetSalesOverTimeQueryHandler : IRequestHandler<GetSalesOverTimeQuery, List<SalesOverTimeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSalesOverTimeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SalesOverTimeDto>> Handle(GetSalesOverTimeQuery request, CancellationToken cancellationToken)
    {
        var fromDate = request.FromDate ?? DateTime.UtcNow.AddMonths(-6);
        var toDate = request.ToDate ?? DateTime.UtcNow;
        var periods = request.Periods;

        var orders = await _context.Set<Order>()
            .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate)
            .ToListAsync(cancellationToken);

        var totalDays = (toDate - fromDate).Days;
        var daysPerPeriod = totalDays / periods;

        var result = new List<SalesOverTimeDto>();

        for (int i = 0; i < periods; i++)
        {
            var periodStart = fromDate.AddDays(i * daysPerPeriod);
            var periodEnd = i == periods - 1 ? toDate : fromDate.AddDays((i + 1) * daysPerPeriod);

            var periodSales = orders
                .Where(o => o.CreatedAt >= periodStart && o.CreatedAt < periodEnd)
                .Sum(o => o.Total);

            var periodLabel = periodStart.ToString("MMM yyyy");

            result.Add(new SalesOverTimeDto
            {
                Period = periodLabel,
                Sales = periodSales
            });
        }

        return result;
    }
}
