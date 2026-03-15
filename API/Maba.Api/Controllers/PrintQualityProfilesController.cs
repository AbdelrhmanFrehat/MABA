using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Printing.PrintQualityProfiles.Commands;
using Maba.Application.Features.Printing.PrintQualityProfiles.DTOs;
using Maba.Application.Features.Printing.PrintQualityProfiles.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PrintQualityProfilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PrintQualityProfilesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all print quality profiles
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<PrintQualityProfileDto>>> GetAll([FromQuery] bool activeOnly = false)
    {
        var query = new GetAllPrintQualityProfilesQuery { ActiveOnly = activeOnly };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a print quality profile by ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<PrintQualityProfileDto>> GetById(Guid id)
    {
        var query = new GetPrintQualityProfileByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Create a new print quality profile
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PrintQualityProfileDto>> Create([FromBody] CreatePrintQualityProfileCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update a print quality profile
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<PrintQualityProfileDto>> Update(Guid id, [FromBody] UpdatePrintQualityProfileCommand command)
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
    /// Delete a print quality profile
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> Delete(Guid id)
    {
        var command = new DeletePrintQualityProfileCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }
}
