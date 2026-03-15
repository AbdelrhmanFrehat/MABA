using MediatR;
using Maba.Application.Features.Catalog.Items.DTOs;

namespace Maba.Application.Features.Catalog.Inventory.Commands;

public class UpdateInventoryCommand : IRequest<InventoryDto>
{
    public Guid ItemId { get; set; }
    public int QuantityOnHand { get; set; }
    public int ReorderLevel { get; set; }
}

