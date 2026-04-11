using MediatR;
using Maba.Application.Features.Machines.DTOs;

namespace Maba.Application.Features.Machines.Queries;

public class GetMachinePartByIdQuery : IRequest<MachinePartDto>
{
    public Guid Id { get; set; }
}
