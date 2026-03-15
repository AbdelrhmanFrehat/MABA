using MediatR;
using Maba.Application.Features.Orders.DTOs;

namespace Maba.Application.Features.Cart.Commands;

public class CheckoutCartCommand : IRequest<OrderDto>
{
    public Guid UserId { get; set; }
    public string? SessionId { get; set; }
    public string ShippingAddressJson { get; set; } = string.Empty;
    public string BillingAddressJson { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = "Cash";
    public string ShippingMethod { get; set; } = "Standard";
    public string? Notes { get; set; }
}
