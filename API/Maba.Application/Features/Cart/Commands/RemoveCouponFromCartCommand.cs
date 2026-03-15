using MediatR;
using Maba.Application.Features.Cart.DTOs;

namespace Maba.Application.Features.Cart.Commands;

public class RemoveCouponFromCartCommand : IRequest<CartDto>
{
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; }
}
