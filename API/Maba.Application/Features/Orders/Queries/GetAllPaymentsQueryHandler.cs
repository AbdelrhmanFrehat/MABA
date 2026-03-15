using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Orders.DTOs;
using Maba.Application.Features.Orders.Queries;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Orders.Handlers;

public class GetAllPaymentsQueryHandler : IRequestHandler<GetAllPaymentsQuery, List<PaymentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllPaymentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PaymentDto>> Handle(GetAllPaymentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Payment>()
            .Include(p => p.PaymentMethod)
            .AsQueryable();

        if (request.OrderId.HasValue)
        {
            query = query.Where(p => p.OrderId == request.OrderId.Value);
        }

        if (request.InvoiceId.HasValue)
        {
            query = query.Where(p => p.InvoiceId == request.InvoiceId.Value);
        }

        var payments = await query.ToListAsync(cancellationToken);

        return payments.Select(p => new PaymentDto
        {
            Id = p.Id,
            OrderId = p.OrderId,
            InvoiceId = p.InvoiceId,
            PaymentMethodId = p.PaymentMethodId,
            PaymentMethodKey = p.PaymentMethod.Key,
            Amount = p.Amount,
            Currency = p.Currency,
            PaidAt = p.PaidAt,
            RefNo = p.RefNo,
            CreatedAt = p.CreatedAt
        }).ToList();
    }
}

