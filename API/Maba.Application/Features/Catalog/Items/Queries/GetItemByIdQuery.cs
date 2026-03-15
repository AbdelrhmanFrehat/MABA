using MediatR;
using Maba.Application.Features.Catalog.Items.DTOs;

namespace Maba.Application.Features.Catalog.Items.Queries;

public class GetItemByIdQuery : IRequest<ItemDto>
{
    public Guid Id { get; set; }
}

