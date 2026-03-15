using MediatR;

namespace Maba.Application.Features.Machines.Commands;

public class DeleteItemMachineLinkCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}
