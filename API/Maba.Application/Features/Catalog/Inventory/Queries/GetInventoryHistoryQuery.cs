using MediatR;
using Maba.Application.Features.Catalog.Items.DTOs;

namespace Maba.Application.Features.Catalog.Inventory.Queries;

public class GetInventoryHistoryQuery : IRequest<List<InventoryTransactionDto>>
{
    public Guid ItemId { get; set; }
}

public class InventoryTransactionDto
{
    public Guid Id { get; set; }
    public Guid InventoryId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal? CostPerUnit { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public Guid? OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
}

