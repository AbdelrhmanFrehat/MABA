using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Common.Dashboard.DTOs;
using Maba.Application.Features.Common.Dashboard.Queries;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Common.Dashboard.Handlers;

public class GetSalesOverTimeQueryHandler : IRequestHandler<GetSalesOverTimeQuery, List<SalesOverTimeDto>>
{
    private const string CancelledKey = "Cancelled";

    private readonly IApplicationDbContext _context;

    public GetSalesOverTimeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SalesOverTimeDto>> Handle(GetSalesOverTimeQuery request, CancellationToken cancellationToken)
    {
        var fromDate = request.FromDate ?? DateTime.UtcNow.AddMonths(-6);
        var toDate = request.ToDate ?? DateTime.UtcNow;
        var periods = Math.Max(1, request.Periods);

        var orders = await _context.Set<Order>()
            .AsNoTracking()
            .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate)
            .Where(o => o.OrderStatus.Key != CancelledKey)
            .Where(o => o.Payments.Sum(p => p.Amount) >= o.Total)
            .Select(o => new { o.CreatedAt, o.Total })
            .ToListAsync(cancellationToken);

        var rangeTicks = Math.Max(1L, toDate.Ticks - fromDate.Ticks);
        var result = new List<SalesOverTimeDto>();

        for (var i = 0; i < periods; i++)
        {
            var bucketStart = new DateTime(fromDate.Ticks + rangeTicks * i / periods, fromDate.Kind);
            DateTime bucketEndExclusive;
            if (i == periods - 1)
            {
                bucketEndExclusive = toDate.AddTicks(1);
            }
            else
            {
                bucketEndExclusive = new DateTime(fromDate.Ticks + rangeTicks * (i + 1) / periods, fromDate.Kind);
            }

            decimal periodSales;
            if (i == periods - 1)
            {
                periodSales = orders
                    .Where(o => o.CreatedAt >= bucketStart && o.CreatedAt <= toDate)
                    .Sum(o => o.Total);
            }
            else
            {
                periodSales = orders
                    .Where(o => o.CreatedAt >= bucketStart && o.CreatedAt < bucketEndExclusive)
                    .Sum(o => o.Total);
            }

            var labelEnd = i == periods - 1 ? toDate : bucketEndExclusive.AddTicks(-1);
            var periodLabel = $"{bucketStart.ToString("MMM d", CultureInfo.InvariantCulture)} – {labelEnd.ToString("MMM d", CultureInfo.InvariantCulture)}";
            if (bucketStart.Year != labelEnd.Year)
            {
                periodLabel = $"{bucketStart.ToString("MMM d yyyy", CultureInfo.InvariantCulture)} – {labelEnd.ToString("MMM d yyyy", CultureInfo.InvariantCulture)}";
            }

            result.Add(new SalesOverTimeDto
            {
                Period = periodLabel,
                Sales = periodSales
            });
        }

        return result;
    }
}
