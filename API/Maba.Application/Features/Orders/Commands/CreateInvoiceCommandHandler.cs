using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Orders.Commands;
using Maba.Application.Features.Orders.DTOs;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Orders.Handlers;

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, InvoiceDto>
{
    private readonly IApplicationDbContext _context;

    public CreateInvoiceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InvoiceDto> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Set<Order>()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found");
        }

        var issuedStatus = await _context.Set<InvoiceStatus>()
            .FirstOrDefaultAsync(s => s.Key == "Issued", cancellationToken);

        if (issuedStatus == null)
        {
            throw new KeyNotFoundException("Issued invoice status not found");
        }

        var invoiceCount = await _context.Set<Invoice>()
            .CountAsync(i => i.OrderId == request.OrderId, cancellationToken);

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            InvoiceNumber = $"INV-{DateTime.UtcNow.Year}-{(invoiceCount + 1):D6}",
            IssueDate = request.IssueDate ?? DateTime.UtcNow,
            DueDate = request.DueDate ?? DateTime.UtcNow.AddDays(30),
            Total = order.Total,
            Currency = order.Currency,
            InvoiceStatusId = issuedStatus.Id
        };

        _context.Set<Invoice>().Add(invoice);
        await _context.SaveChangesAsync(cancellationToken);

        return new InvoiceDto
        {
            Id = invoice.Id,
            OrderId = invoice.OrderId,
            InvoiceNumber = invoice.InvoiceNumber,
            IssueDate = invoice.IssueDate,
            DueDate = invoice.DueDate,
            Total = invoice.Total,
            Currency = invoice.Currency,
            InvoiceStatusId = invoice.InvoiceStatusId,
            InvoiceStatusKey = issuedStatus.Key,
            CreatedAt = invoice.CreatedAt,
            UpdatedAt = invoice.UpdatedAt
        };
    }
}

