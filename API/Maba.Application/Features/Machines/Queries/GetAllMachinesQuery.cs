using MediatR;
using Maba.Application.Features.Machines.DTOs;

namespace Maba.Application.Features.Machines.Queries;

public class GetAllMachinesQuery : IRequest<List<MachineDto>>
{
}

