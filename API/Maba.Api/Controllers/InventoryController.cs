using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.BusinessInventory.Queries;
using Maba.Application.Features.Catalog.Inventory.Commands;
using Maba.Application.Features.Catalog.Inventory.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("stock")]
    public async Task<ActionResult> GetStockOverview()
    {
        var result = await _mediator.Send(new GetStockOverviewQuery());
        return Ok(result);
    }

    [HttpGet("item/{itemId}")]
    public async Task<ActionResult> GetInventoryByItemId(Guid itemId)
    {
        var query = new GetInventoryByItemIdQuery { ItemId = itemId };
        var result = await _mediator.Send(query);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [HttpPut("item/{itemId}")]
    public async Task<ActionResult> UpdateInventory(Guid itemId, [FromBody] UpdateInventoryCommand command)
    {
        command.ItemId = itemId;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("item/{itemId}/adjust")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> AdjustInventory(Guid itemId, [FromBody] AdjustInventoryCommand command)
    {
        command.ItemId = itemId;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("item/{itemId}/reserve")]
    public async Task<ActionResult> ReserveInventory(Guid itemId, [FromBody] ReserveInventoryCommand command)
    {
        command.ItemId = itemId;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("item/{itemId}/release")]
    public async Task<ActionResult> ReleaseInventory(Guid itemId, [FromBody] ReleaseInventoryCommand command)
    {
        command.ItemId = itemId;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("item/{itemId}/history")]
    public async Task<ActionResult> GetInventoryHistory(Guid itemId)
    {
        var query = new GetInventoryHistoryQuery { ItemId = itemId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("low-stock")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> GetLowStockItems()
    {
        var query = new GetLowStockItemsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

