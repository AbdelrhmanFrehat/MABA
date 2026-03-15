using MediatR;

namespace Maba.Application.Features.Cnc.Requests.Commands;

public class DeleteCncServiceRequestCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
