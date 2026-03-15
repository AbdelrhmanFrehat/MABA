using MediatR;
using Maba.Application.Features.Catalog.Items.DTOs;

namespace Maba.Application.Features.Catalog.Items.Queries;

public class GetItemDetailQuery : IRequest<ItemDetailDto>
{
    public Guid ItemId { get; set; }
}

