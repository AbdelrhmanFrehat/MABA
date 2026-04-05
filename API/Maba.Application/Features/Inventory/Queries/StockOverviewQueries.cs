using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Catalog;
using WarehouseEntity = Maba.Domain.Inventory.Warehouse;

namespace Maba.Application.Features.BusinessInventory.Queries;

public class GetStockOverviewQuery : IRequest<List<StockOverviewItemDto>>
{
}

public class StockOverviewItemDto
{
    public Guid Id { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ItemSku { get; set; } = string.Empty;
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public int QuantityOnOrder { get; set; }
    public int QuantityAvailable { get; set; }
    public int ReorderLevel { get; set; }
    public decimal CostPerUnit { get; set; }
    public DateTime? LastStockInAt { get; set; }
    public DateTime? LastStockOutAt { get; set; }
}

public class GetStockOverviewQueryHandler : IRequestHandler<GetStockOverviewQuery, List<StockOverviewItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStockOverviewQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StockOverviewItemDto>> Handle(GetStockOverviewQuery request, CancellationToken cancellationToken)
    {
        var warehousesById = await _context.Set<WarehouseEntity>()
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.NameEn, cancellationToken);

        var inventoryRows = await _context.Set<Inventory>()
            .AsNoTracking()
            .Include(x => x.Item)
            .OrderBy(x => x.WarehouseId)
            .ThenBy(x => x.Item.NameEn)
            .Select(x => new StockOverviewItemDto
            {
                Id = x.Id,
                WarehouseId = x.WarehouseId,
                ItemId = x.ItemId,
                ItemName = x.Item.NameEn,
                ItemSku = x.Item.Sku,
                QuantityOnHand = x.QuantityOnHand,
                QuantityReserved = x.QuantityReserved,
                QuantityOnOrder = x.QuantityOnOrder,
                QuantityAvailable = x.QuantityOnHand - x.QuantityReserved,
                ReorderLevel = x.ReorderLevel,
                CostPerUnit = x.CostPerUnit ?? 0,
                LastStockInAt = x.LastStockInAt,
                LastStockOutAt = x.LastStockOutAt
            })
            .ToListAsync(cancellationToken);

        foreach (var row in inventoryRows)
        {
            if (row.WarehouseId.HasValue && warehousesById.TryGetValue(row.WarehouseId.Value, out var warehouseName))
            {
                row.WarehouseName = warehouseName;
            }
        }

        return inventoryRows;
    }
}
