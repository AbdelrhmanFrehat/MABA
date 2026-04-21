using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maba.Application.Features.MachineCatalog.DTOs;
using Maba.Application.Features.MachineCatalog.Handlers;

namespace Maba.Api.Controllers.MachineCatalog;

/// <summary>
/// Machine catalog families — app-facing read + admin write.
/// </summary>
[ApiController]
[Route("api/v1/machine-families")]
public class MachineFamiliesController : ControllerBase
{
    private readonly IMediator _mediator;
    public MachineFamiliesController(IMediator mediator) => _mediator = mediator;

    // ── App-facing ────────────────────────────────────────────────────────

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<MachineFamilyDto>>> GetAll(
        [FromQuery] Guid? categoryId,
        [FromQuery] bool? isActive)
        => Ok(await _mediator.Send(new GetMachineFamiliesQuery(categoryId, isActive)));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<MachineFamilyDto>> GetById(Guid id)
        => Ok(await _mediator.Send(new GetMachineFamilyByIdQuery(id)));

    // ── Admin write ───────────────────────────────────────────────────────

    [HttpPost]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult<MachineFamilyDto>> Create([FromBody] CreateMachineFamilyRequest request)
    {
        var result = await _mediator.Send(new CreateMachineFamilyCommand(request));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult<MachineFamilyDto>> Update(Guid id, [FromBody] UpdateMachineFamilyRequest request)
    {
        if (id != request.Id) return BadRequest(new { message = "ID mismatch." });
        return Ok(await _mediator.Send(new UpdateMachineFamilyCommand(request)));
    }

    [HttpPatch("{id:guid}/toggle-active")]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult> ToggleActive(Guid id)
    {
        var success = await _mediator.Send(new ToggleMachineFamilyActiveCommand(id));
        return success ? NoContent() : NotFound();
    }
}
