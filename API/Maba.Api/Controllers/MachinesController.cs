using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Machines.Commands;
using Maba.Application.Features.Machines.DTOs;
using Maba.Application.Features.Machines.Queries;
using Maba.Application.Common.Models;
using System.ComponentModel.DataAnnotations;

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

    /// <summary>
    /// GET /api/v1/machines/parts — paginated list of all machine parts (optionally filtered by machineId or search).
    /// This route must be declared before GET {id}/parts to avoid ASP.NET route ambiguity.
    /// </summary>
    [HttpGet("parts")]
    public async Task<ActionResult<GetAllMachinePartsResult>> GetAllMachineParts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? machineId = null,
        [FromQuery] string? search = null)
    {
        var query = new GetAllMachinePartsQuery
        {
            Page = page,
            PageSize = pageSize,
            MachineId = machineId,
            Search = search
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("parts/{id:guid}")]
    public async Task<ActionResult<MachinePartDto>> GetMachinePartById(Guid id)
    {
        var result = await _mediator.Send(new GetMachinePartByIdQuery { Id = id });
        return Ok(result);
    }

    [HttpPost("parts")]
    public async Task<ActionResult<MachinePartDto>> CreateMachinePart([FromBody] CreateMachinePartCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("parts/{id:guid}")]
    public async Task<ActionResult<MachinePartDto>> UpdateMachinePart(Guid id, [FromBody] UpdateMachinePartCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("parts/{id:guid}")]
    public async Task<ActionResult> DeleteMachinePart(Guid id)
    {
        await _mediator.Send(new DeleteMachinePartCommand { Id = id });
        return NoContent();
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

