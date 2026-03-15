using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Inventory.Queries;
using Maba.Domain.Catalog;
using InventoryEntity = Maba.Domain.Catalog.Inventory;

namespace Maba.Application.Features.Catalog.Inventory.Handlers;

public class GetInventoryHistoryQueryHandler : IRequestHandler<GetInventoryHistoryQuery, List<InventoryTransactionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetInventoryHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<InventoryTransactionDto>> Handle(GetInventoryHistoryQuery request, CancellationToken cancellationToken)
    {
        var inventory = await _context.Set<InventoryEntity>()
            .FirstOrDefaultAsync(i => i.ItemId == request.ItemId, cancellationToken);

        if (inventory == null)
        {
            return new List<InventoryTransactionDto>();
        }

        var transactions = await _context.Set<InventoryTransaction>()
            .Where(t => t.InventoryId == inventory.Id)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return transactions.Select(t => new InventoryTransactionDto
        {
            Id = t.Id,
            InventoryId = t.InventoryId,
            TransactionType = t.TransactionType,
            Quantity = t.Quantity,
            CostPerUnit = t.CostPerUnit,
            Reason = t.Reason,
            Notes = t.Notes,
            OrderId = t.OrderId,
            CreatedAt = t.CreatedAt
        }).ToList();
    }
}

