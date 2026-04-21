using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maba.Application.Features.MachineCatalog.DTOs;
using Maba.Application.Features.MachineCatalog.Handlers;

namespace Maba.Api.Controllers.MachineCatalog;

/// <summary>
/// Machine catalog categories — app-facing read + admin write.
/// </summary>
[ApiController]
[Route("api/v1/machine-categories")]
public class MachineCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;
    public MachineCategoriesController(IMediator mediator) => _mediator = mediator;

    // ── App-facing ────────────────────────────────────────────────────────

    /// <summary>Returns all active machine categories for the app catalog/wizard.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<MachineCategoryDto>>> GetAll([FromQuery] bool? isActive)
        => Ok(await _mediator.Send(new GetMachineCategoriesQuery(isActive)));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<MachineCategoryDto>> GetById(Guid id)
        => Ok(await _mediator.Send(new GetMachineCategoryByIdQuery(id)));

    // ── Admin write ───────────────────────────────────────────────────────

    [HttpPost]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult<MachineCategoryDto>> Create([FromBody] CreateMachineCategoryRequest request)
    {
        var result = await _mediator.Send(new CreateMachineCategoryCommand(request));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult<MachineCategoryDto>> Update(Guid id, [FromBody] UpdateMachineCategoryRequest request)
    {
        if (id != request.Id) return BadRequest(new { message = "ID mismatch." });
        return Ok(await _mediator.Send(new UpdateMachineCategoryCommand(request)));
    }

    [HttpPatch("{id:guid}/toggle-active")]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult> ToggleActive(Guid id)
    {
        var success = await _mediator.Send(new ToggleMachineCategoryActiveCommand(id));
        return success ? NoContent() : NotFound();
    }
}
