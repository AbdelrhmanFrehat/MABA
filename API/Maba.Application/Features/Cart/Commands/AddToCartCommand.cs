using MediatR;
using Maba.Application.Features.Cart.DTOs;

namespace Maba.Application.Features.Cart.Commands;

public class AddToCartCommand : IRequest<CartDto>
{
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; }
    public Guid ItemId { get; set; }
    public int Quantity { get; set; } = 1;
}
