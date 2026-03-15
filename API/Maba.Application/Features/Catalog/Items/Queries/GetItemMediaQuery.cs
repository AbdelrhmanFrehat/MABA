using MediatR;
using Maba.Application.Features.Catalog.Items.Commands;

namespace Maba.Application.Features.Catalog.Items.Queries;

public class GetItemMediaQuery : IRequest<List<ItemMediaDto>>
{
    public Guid ItemId { get; set; }
}
