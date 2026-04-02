using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Finance.DTOs;
using Maba.Application.Features.Finance.Queries;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Finance.Handlers;

public class GetFinanceRevenueByMonthQueryHandler : IRequestHandler<GetFinanceRevenueByMonthQuery, FinanceChartDto>
{
    private readonly IApplicationDbContext _context;

    public GetFinanceRevenueByMonthQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FinanceChartDto> Handle(GetFinanceRevenueByMonthQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-11);

        var payments = await _context.Set<Payment>()
            .AsNoTracking()
            .Where(p => !p.IsRefunded && p.PaidAt >= start)
            .ToListAsync(cancellationToken);
        var labels = new List<string>();
        var values = new List<decimal>();

        for (var i = 0; i < 12; i++)
        {
            var monthStart = start.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1);
            var sum = payments
                .Where(p => p.PaidAt >= monthStart && p.PaidAt < monthEnd)
                .Sum(p => p.Amount);
            labels.Add(monthStart.ToString("MMM yyyy", CultureInfo.InvariantCulture));
            values.Add(sum);
        }

        return new FinanceChartDto { Labels = labels, Values = values };
    }
}
