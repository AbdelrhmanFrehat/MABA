using MediatR;

namespace Maba.Application.Features.Machines.Commands;

public class DeleteMachinePartCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}
