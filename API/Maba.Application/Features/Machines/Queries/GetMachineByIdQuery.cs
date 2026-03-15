using MediatR;
using Maba.Application.Features.Machines.DTOs;

namespace Maba.Application.Features.Machines.Queries;

public class GetMachineByIdQuery : IRequest<MachineDto>
{
    public Guid Id { get; set; }
}

