using MediatR;

namespace Maba.Application.Features.Orders.Commands;

public class AddOrderNoteCommand : IRequest<Unit>
{
    public Guid OrderId { get; set; }
    public string Note { get; set; } = string.Empty;
}

