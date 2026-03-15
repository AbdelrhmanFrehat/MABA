using MediatR;
using Maba.Application.Features.Cart.DTOs;

namespace Maba.Application.Features.Cart.Commands;

public class ApplyCouponToCartCommand : IRequest<CartDto>
{
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; }
    public string CouponCode { get; set; } = string.Empty;
}
