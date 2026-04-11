using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Orders.Commands;
using Maba.Application.Features.Orders.DTOs;
using Maba.Domain.Orders;
using Microsoft.Extensions.Logging;

namespace Maba.Application.Features.Orders.Handlers;

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, PaymentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IAccountingPostingService _posting;
    private readonly ILogger<CreatePaymentCommandHandler> _logger;

    public CreatePaymentCommandHandler(
        IApplicationDbContext context,
        IAccountingPostingService posting,
        ILogger<CreatePaymentCommandHandler> logger)
    {
        _context = context;
        _posting = posting;
        _logger = logger;
    }

    public async Task<PaymentDto> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Set<Order>()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException("Order not found");

        var paymentMethod = await _context.Set<PaymentMethod>()
            .FirstOrDefaultAsync(pm => pm.Id == request.PaymentMethodId, cancellationToken)
            ?? throw new KeyNotFoundException("Payment method not found");

        var payment = new Payment
        {
            Id              = Guid.NewGuid(),
            OrderId         = request.OrderId,
            InvoiceId       = request.InvoiceId,
            PaymentMethodId = request.PaymentMethodId,
            Amount          = request.Amount,
            Currency        = request.Currency,
            PaidAt          = DateTime.UtcNow,
            RefNo           = request.RefNo
        };

        _context.Set<Payment>().Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        // Post accounting journal entry: DR Cash / CR AR
        try
        {
            await _posting.PostPaymentAsync(payment.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log but do not fail the payment — accounting can be reconciled separately
            _logger.LogWarning(ex, "Accounting posting failed for payment {PaymentId}. Payment saved.", payment.Id);
        }

        return new PaymentDto
        {
            Id              = payment.Id,
            OrderId         = payment.OrderId,
            InvoiceId       = payment.InvoiceId,
            PaymentMethodId = payment.PaymentMethodId,
            PaymentMethodKey = paymentMethod.Key,
            Amount          = payment.Amount,
            Currency        = payment.Currency,
            PaidAt          = payment.PaidAt,
            RefNo           = payment.RefNo,
            CreatedAt       = payment.CreatedAt
        };
    }
}

