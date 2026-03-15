using MediatR;

namespace Maba.Application.Features.Catalog.Items.Commands;

public class UnlinkMediaFromItemCommand : IRequest<Unit>
{
    public Guid ItemId { get; set; }
    public Guid MediaLinkId { get; set; }
}
