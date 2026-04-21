using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maba.Application.Features.MachineCatalog.DTOs;
using Maba.Application.Features.MachineCatalog.Handlers;
using Maba.Domain.MachineCatalog.Sections;

namespace Maba.Api.Controllers.MachineCatalog;

/// <summary>
/// Machine definitions — app-facing read + admin write.
///
/// Visibility rules:
///   isActive=false  → excluded from all app responses (activeOnly=true default)
///   isPublic=false  → excluded from app responses; admin-only via adminMode param
///   isDeprecated    → returned with flag; excluded from app wizard by default
///
/// internalNotes is NEVER returned in app-facing (non-admin) requests.
/// </summary>
[ApiController]
[Route("api/v1/machine-definitions")]
public class MachineDefinitionsController : ControllerBase
{
    private readonly IMediator _mediator;
    public MachineDefinitionsController(IMediator mediator) => _mediator = mediator;

    // ── App-facing read ───────────────────────────────────────────────────

    /// <summary>
    /// List machine definitions (summary only). App setup wizard uses this.
    /// Defaults: activeOnly=true, includeDeprecated=false, publicOnly.
    /// Admin clients pass adminMode=true to see all including private + deprecated.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<MachineDefinitionSummaryDto>>> GetList(
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? familyId,
        [FromQuery] bool activeOnly = true,
        [FromQuery] bool includeDeprecated = false,
        [FromQuery] string? search = null)
    {
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("StoreOwner");
        var result = await _mediator.Send(new GetMachineDefinitionsQuery(new MachineDefinitionListQuery
        {
            CategoryId = categoryId,
            FamilyId = familyId,
            ActiveOnly = activeOnly,
            IncludeDeprecated = includeDeprecated,
            AdminMode = isAdmin,
            Search = search
        }));
        return Ok(result);
    }

    /// <summary>
    /// Get full machine definition (all sections). Used by app to cache definitions.
    /// internalNotes excluded unless caller is admin.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<MachineDefinitionDto>> GetById(Guid id)
    {
        var includeInternal = User.IsInRole("Admin") || User.IsInRole("StoreOwner");
        return Ok(await _mediator.Send(new GetMachineDefinitionByIdQuery(id, includeInternal)));
    }

    /// <summary>
    /// Get only the capabilities section. Lightweight endpoint for driver binding checks.
    /// </summary>
    [HttpGet("{id:guid}/capabilities")]
    [AllowAnonymous]
    public async Task<ActionResult<CapabilitiesSection>> GetCapabilities(Guid id)
        => Ok(await _mediator.Send(new GetMachineDefinitionCapabilitiesQuery(id)));

    // ── Admin write ───────────────────────────────────────────────────────

    [HttpPost]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult<MachineDefinitionDto>> Create([FromBody] CreateMachineDefinitionRequest request)
    {
        var result = await _mediator.Send(new CreateMachineDefinitionCommand(request));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult<MachineDefinitionDto>> Update(Guid id, [FromBody] UpdateMachineDefinitionRequest request)
    {
        if (id != request.Id) return BadRequest(new { message = "ID mismatch." });
        return Ok(await _mediator.Send(new UpdateMachineDefinitionCommand(request)));
    }

    /// <summary>
    /// Patch status fields only: isActive, isPublic, isDeprecated, deprecationNote.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult> PatchStatus(Guid id, [FromBody] PatchMachineDefinitionStatusRequest request)
    {
        var success = await _mediator.Send(new PatchMachineDefinitionStatusCommand(id, request));
        return success ? NoContent() : NotFound();
    }

    /// <summary>
    /// Soft delete — sets isActive=false. Definition record is preserved.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var success = await _mediator.Send(new SoftDeleteMachineDefinitionCommand(id));
        return success ? NoContent() : NotFound();
    }
}
