using MediatR;
using Maba.Application.Features.Catalog.Items.DTOs;

namespace Maba.Application.Features.Catalog.Inventory.Queries;

public class GetLowStockItemsQuery : IRequest<List<LowStockItemDto>>
{
}

public class LowStockItemDto
{
    public Guid ItemId { get; set; }
    public string ItemNameEn { get; set; } = string.Empty;
    public string ItemSku { get; set; } = string.Empty;
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public int QuantityAvailable { get; set; }
    public int ReorderLevel { get; set; }
    public bool IsBelowReorderLevel { get; set; }
}

