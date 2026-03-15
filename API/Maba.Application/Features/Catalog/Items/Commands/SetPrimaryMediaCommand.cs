using MediatR;

namespace Maba.Application.Features.Catalog.Items.Commands;

public class SetPrimaryMediaCommand : IRequest<Unit>
{
    public Guid ItemId { get; set; }
    public Guid MediaLinkId { get; set; }
}
