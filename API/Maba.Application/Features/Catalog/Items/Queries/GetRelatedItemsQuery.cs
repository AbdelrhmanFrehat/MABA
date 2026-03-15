using MediatR;
using Maba.Application.Features.Catalog.Items.DTOs;

namespace Maba.Application.Features.Catalog.Items.Queries;

public class GetRelatedItemsQuery : IRequest<List<ItemDto>>
{
    public Guid ItemId { get; set; }
    public int? Limit { get; set; } = 10;
}

