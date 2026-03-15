using MediatR;

namespace Maba.Application.Features.Cart.Commands;

public class ClearCartCommand : IRequest<Unit>
{
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; }
}
