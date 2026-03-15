using MediatR;
using Maba.Application.Features.Machines.DTOs;

namespace Maba.Application.Features.Machines.Queries;

public class GetMachinesByItemQuery : IRequest<List<MachineDto>>
{
    public Guid ItemId { get; set; }
}

