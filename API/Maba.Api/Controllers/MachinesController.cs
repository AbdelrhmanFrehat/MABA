using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Machines.Commands;
using Maba.Application.Features.Machines.DTOs;
using Maba.Application.Features.Machines.Queries;
using Maba.Application.Common.Models;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class MachinesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MachinesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<MachineDto>>> GetAllMachines()
    {
        var query = new GetAllMachinesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MachineDto>> GetMachineById(Guid id)
    {
        var query = new GetMachineByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<MachineDto>> CreateMachine([FromBody] CreateMachineCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetMachineById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MachineDto>> UpdateMachine(Guid id, [FromBody] UpdateMachineCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMachine(Guid id)
    {
        var command = new DeleteMachineCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("parts")]
    public async Task<ActionResult<MachinePartDto>> CreateMachinePart([FromBody] CreateMachinePartCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("links")]
    public async Task<ActionResult<ItemMachineLinkDto>> CreateItemMachineLink([FromBody] CreateItemMachineLinkCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("links")]
    public async Task<ActionResult<List<ItemMachineLinkDto>>> GetItemMachineLinks([FromQuery] Guid? itemId, [FromQuery] Guid? machineId)
    {
        var query = new GetItemMachineLinksQuery { ItemId = itemId, MachineId = machineId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpDelete("item-machine-links/{id}")]
    public async Task<ActionResult> DeleteItemMachineLink(Guid id)
    {
        var command = new DeleteItemMachineLinkCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpGet("{id}/detail")]
    [AllowAnonymous]
    public async Task<ActionResult<MachineDetailDto>> GetMachineDetail(Guid id)
    {
        var query = new GetMachineDetailQuery { MachineId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}/parts")]
    [AllowAnonymous]
    public async Task<ActionResult<List<MachinePartDto>>> GetMachineParts(Guid id)
    {
        var query = new GetMachinePartsQuery { MachineId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<MachineDto>>> SearchMachines(
        [FromQuery] string? searchTerm,
        [FromQuery] string? manufacturer,
        [FromQuery] string? model,
        [FromQuery] int? yearFrom,
        [FromQuery] int? yearTo,
        [FromQuery] string? location,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var query = new SearchMachinesQuery
        {
            SearchTerm = searchTerm,
            Manufacturer = manufacturer,
            Model = model,
            YearFrom = yearFrom,
            YearTo = yearTo,
            Location = location,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDescending = sortDescending
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

