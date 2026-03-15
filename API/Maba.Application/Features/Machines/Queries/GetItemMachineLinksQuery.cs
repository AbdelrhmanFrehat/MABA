using MediatR;
using Maba.Application.Features.Machines.DTOs;

namespace Maba.Application.Features.Machines.Queries;

public class GetItemMachineLinksQuery : IRequest<List<ItemMachineLinkDto>>
{
    public Guid? ItemId { get; set; }
    public Guid? MachineId { get; set; }
}

