using MediatR;
using Maba.Application.Features.Catalog.Items.DTOs;

namespace Maba.Application.Features.Catalog.Inventory.Queries;

public class GetInventoryByItemIdQuery : IRequest<InventoryDto?>
{
    public Guid ItemId { get; set; }
}

