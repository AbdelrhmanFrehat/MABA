using MediatR;
using Maba.Application.Features.Machines.DTOs;

namespace Maba.Application.Features.Machines.Queries;

public class GetMachinePartsQuery : IRequest<List<MachinePartDto>>
{
    public Guid MachineId { get; set; }
}

