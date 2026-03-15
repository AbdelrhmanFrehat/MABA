using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Orders.DTOs;
using Maba.Application.Features.Orders.Queries;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Orders.Handlers;

public class GetAllInvoicesQueryHandler : IRequestHandler<GetAllInvoicesQuery, List<InvoiceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllInvoicesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<InvoiceDto>> Handle(GetAllInvoicesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Invoice>()
            .Include(i => i.InvoiceStatus)
            .AsQueryable();

        if (request.OrderId.HasValue)
        {
            query = query.Where(i => i.OrderId == request.OrderId.Value);
        }

        if (request.InvoiceStatusId.HasValue)
        {
            query = query.Where(i => i.InvoiceStatusId == request.InvoiceStatusId.Value);
        }

        var invoices = await query.ToListAsync(cancellationToken);

        return invoices.Select(i => new InvoiceDto
        {
            Id = i.Id,
            OrderId = i.OrderId,
            InvoiceNumber = i.InvoiceNumber,
            IssueDate = i.IssueDate,
            DueDate = i.DueDate,
            Total = i.Total,
            Currency = i.Currency,
            InvoiceStatusId = i.InvoiceStatusId,
            InvoiceStatusKey = i.InvoiceStatus.Key,
            CreatedAt = i.CreatedAt,
            UpdatedAt = i.UpdatedAt
        }).ToList();
    }
}

