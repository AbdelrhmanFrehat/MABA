using MediatR;
using Maba.Application.Features.Catalog.Items.DTOs;

namespace Maba.Application.Features.Catalog.Inventory.Commands;

public class AdjustInventoryCommand : IRequest<InventoryDto>
{
    public Guid ItemId { get; set; }
    public int QuantityChange { get; set; } // Positive for increase, negative for decrease
    public decimal? CostPerUnit { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public Guid? CreatedByUserId { get; set; }
}

