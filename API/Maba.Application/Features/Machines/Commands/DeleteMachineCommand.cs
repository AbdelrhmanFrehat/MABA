using MediatR;

namespace Maba.Application.Features.Machines.Commands;

public class DeleteMachineCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}

