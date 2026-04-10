using MediatR;
using Maba.Application.Features.Orders.DTOs;

namespace Maba.Application.Features.Orders.Commands;

public class CreatePaymentCommand : IRequest<PaymentDto>
{
    public Guid OrderId { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ILS";
    public string? RefNo { get; set; }
}

