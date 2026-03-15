using MediatR;
using Maba.Application.Features.Machines.DTOs;

namespace Maba.Application.Features.Machines.Queries;

public class GetMachineDetailQuery : IRequest<MachineDetailDto>
{
    public Guid MachineId { get; set; }
}

