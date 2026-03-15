using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Cnc.Materials.Commands;
using Maba.Application.Features.Cnc.Materials.Queries;
using Maba.Application.Features.Cnc.DTOs;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/cnc/materials")]
public class CncMaterialsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CncMaterialsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all CNC materials (public endpoint for frontend).
    /// Always excludes metal materials and returns only active items.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<CncMaterialDto>>> GetAllMaterials(
        [FromQuery] string? type = null)
    {
        var query = new GetAllCncMaterialsQuery
        {
            ActiveOnly = true,
            ExcludeMetal = true,
            Type = type
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get all CNC materials for admin (includes inactive and metal materials)
    /// </summary>
    [HttpGet("admin")]
    [Authorize]
    public async Task<ActionResult<List<CncMaterialDto>>> GetAllMaterialsAdmin(
        [FromQuery] bool? activeOnly = false)
    {
        var query = new GetAllCncMaterialsQuery
        {
            ActiveOnly = activeOnly,
            ExcludeMetal = false
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get CNC material by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<CncMaterialDto>> GetMaterialById(Guid id)
    {
        var query = new GetCncMaterialByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new CNC material (admin only)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CncMaterialDto>> CreateMaterial([FromBody] CreateCncMaterialCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetMaterialById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update a CNC material (admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<CncMaterialDto>> UpdateMaterial(Guid id, [FromBody] UpdateCncMaterialCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID mismatch");
        }

        var result = await _mediator.Send(command);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a CNC material (admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteMaterial(Guid id)
    {
        var command = new DeleteCncMaterialCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}
