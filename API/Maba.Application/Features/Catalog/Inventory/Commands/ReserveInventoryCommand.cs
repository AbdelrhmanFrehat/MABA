using MediatR;
using Maba.Application.Features.Catalog.Items.DTOs;

namespace Maba.Application.Features.Catalog.Inventory.Commands;

public class ReserveInventoryCommand : IRequest<InventoryDto>
{
    public Guid ItemId { get; set; }
    public int Quantity { get; set; }
    public Guid OrderId { get; set; }
}

