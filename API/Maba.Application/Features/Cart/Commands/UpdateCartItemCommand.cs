using MediatR;
using Maba.Application.Features.Cart.DTOs;

namespace Maba.Application.Features.Cart.Commands;

public class UpdateCartItemCommand : IRequest<CartDto>
{
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; }
    public Guid CartItemId { get; set; }
    public int Quantity { get; set; }
}
