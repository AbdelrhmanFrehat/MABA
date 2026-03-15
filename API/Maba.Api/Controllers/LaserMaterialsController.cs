using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Laser.Materials.Commands;
using Maba.Application.Features.Laser.Materials.Queries;
using Maba.Application.Features.Laser.DTOs;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/laser/materials")]
public class LaserMaterialsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LaserMaterialsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all laser materials (public endpoint for frontend).
    /// Always excludes metal materials and returns only active items.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<LaserMaterialDto>>> GetAllMaterials(
        [FromQuery] string? type = null)
    {
        var query = new GetAllLaserMaterialsQuery
        {
            ActiveOnly = true,
            ExcludeMetal = true,
            Type = type
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get all laser materials for admin (includes inactive and metal materials)
    /// </summary>
    [HttpGet("admin")]
    [Authorize]
    public async Task<ActionResult<List<LaserMaterialDto>>> GetAllMaterialsAdmin(
        [FromQuery] bool? activeOnly = false)
    {
        var query = new GetAllLaserMaterialsQuery
        {
            ActiveOnly = activeOnly,
            ExcludeMetal = false
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get laser material by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<LaserMaterialDto>> GetMaterialById(Guid id)
    {
        var query = new GetLaserMaterialByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new laser material (admin only)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<LaserMaterialDto>> CreateMaterial([FromBody] CreateLaserMaterialCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetMaterialById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update a laser material (admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<LaserMaterialDto>> UpdateMaterial(Guid id, [FromBody] UpdateLaserMaterialCommand command)
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
    /// Delete a laser material (admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteMaterial(Guid id)
    {
        var command = new DeleteLaserMaterialCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}
