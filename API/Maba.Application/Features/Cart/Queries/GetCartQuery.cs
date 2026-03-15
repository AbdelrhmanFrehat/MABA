using MediatR;
using Maba.Application.Features.Cart.DTOs;

namespace Maba.Application.Features.Cart.Queries;

public class GetCartQuery : IRequest<CartDto?>
{
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; }
}
