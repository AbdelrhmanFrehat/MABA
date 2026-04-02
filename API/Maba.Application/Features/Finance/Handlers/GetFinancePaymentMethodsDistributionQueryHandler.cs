using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Finance.DTOs;
using Maba.Application.Features.Finance.Queries;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Finance.Handlers;

public class GetFinancePaymentMethodsDistributionQueryHandler
    : IRequestHandler<GetFinancePaymentMethodsDistributionQuery, FinanceChartDto>
{
    private readonly IApplicationDbContext _context;

    public GetFinancePaymentMethodsDistributionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FinanceChartDto> Handle(
        GetFinancePaymentMethodsDistributionQuery request,
        CancellationToken cancellationToken)
    {
        var payments = await _context.Set<Payment>()
            .AsNoTracking()
            .Include(p => p.PaymentMethod)
            .Where(p => !p.IsRefunded)
            .ToListAsync(cancellationToken);

        var grouped = payments
            .GroupBy(p => string.IsNullOrWhiteSpace(p.PaymentMethod.NameEn) ? "Other" : p.PaymentMethod.NameEn)
            .Select(g => new { Label = g.Key, Total = g.Sum(x => x.Amount) })
            .OrderByDescending(x => x.Total)
            .ToList();

        return new FinanceChartDto
        {
            Labels = grouped.Select(x => x.Label).ToList(),
            Values = grouped.Select(x => x.Total).ToList()
        };
    }
}
