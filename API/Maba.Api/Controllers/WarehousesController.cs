using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maba.Application.Features.BusinessInventory.Commands;
using Maba.Application.Features.BusinessInventory.DTOs;
using Maba.Application.Features.BusinessInventory.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class WarehousesController : ControllerBase
{
    private readonly IMediator _mediator;

    public WarehousesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<WarehouseDto>>> GetWarehouses()
    {
        return Ok(await _mediator.Send(new GetWarehousesQuery()));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<WarehouseDto>> CreateWarehouse([FromBody] CreateWarehouseCommand command)
    {
        return Ok(await _mediator.Send(command));
    }
}
