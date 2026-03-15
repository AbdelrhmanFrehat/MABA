using MediatR;
using Maba.Application.Features.Machines.DTOs;

namespace Maba.Application.Features.Machines.Commands;

public class CreateItemMachineLinkCommand : IRequest<ItemMachineLinkDto>
{
    public Guid ItemId { get; set; }
    public Guid MachineId { get; set; }
    public Guid? MachinePartId { get; set; }
}

