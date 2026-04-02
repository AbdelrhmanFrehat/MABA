using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Finance.DTOs;
using Maba.Application.Features.Finance.Queries;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Finance.Handlers;

public class GetFinanceKpisQueryHandler : IRequestHandler<GetFinanceKpisQuery, FinanceKpisDto>
{
    private const string CancelledKey = "Cancelled";

    private readonly IApplicationDbContext _context;

    public GetFinanceKpisQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FinanceKpisDto> Handle(GetFinanceKpisQuery request, CancellationToken cancellationToken)
    {
        var orders = await _context.Set<Order>()
            .AsNoTracking()
            .Include(o => o.OrderStatus)
            .Include(o => o.Payments)
            .Where(o => o.OrderStatus.Key != CancelledKey)
            .ToListAsync(cancellationToken);

        var totalRevenue = orders.Sum(o => o.Total);

        var payments = await _context.Set<Payment>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalPaid = payments.Where(p => !p.IsRefunded).Sum(p => p.Amount);

        var outstanding = orders.Sum(o =>
        {
            var paid = o.Payments?.Where(p => !p.IsRefunded).Sum(p => p.Amount) ?? 0;
            return Math.Max(0, o.Total - paid);
        });

        var refunds = payments.Where(p => p.IsRefunded).Sum(p => p.RefundedAmount ?? p.Amount);

        return new FinanceKpisDto
        {
            TotalRevenue = totalRevenue,
            TotalPaid = totalPaid,
            Outstanding = outstanding,
            Refunds = refunds
        };
    }
}
