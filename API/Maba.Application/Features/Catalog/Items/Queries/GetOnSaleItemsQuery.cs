using MediatR;
using Maba.Application.Features.Catalog.Items.DTOs;

namespace Maba.Application.Features.Catalog.Items.Queries;

public class GetOnSaleItemsQuery : IRequest<List<ItemDto>>
{
    public int? Limit { get; set; } = 10;
}

