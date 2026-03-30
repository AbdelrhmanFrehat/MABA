using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Maba.Api.Authorization;
using Maba.Application.Features.ControlCenter.Instances.Commands;
using Maba.Application.Features.ControlCenter.Instances.Queries;

namespace Maba.Api.Controllers.ControlCenter;

[ApiController]
[Route("api/v1/control-center/[controller]")]
[Authorize]
public class InstancesController : ControllerBase
{
    private readonly IMediator _mediator;

    public InstancesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetOrgIdOrThrow()
    {
        // For now, use NameIdentifier as OrgId placeholder if no dedicated claim exists.
        var orgClaim = User.FindFirst("orgId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        if (orgClaim == null || !Guid.TryParse(orgClaim.Value, out var orgId))
        {
            throw new UnauthorizedAccessException("Org context is missing from token.");
        }

        return orgId;
    }

    /// <summary>
    /// Register a new Control Center instance.
    /// </summary>
    [HttpPost("register")]
    [Authorize(Policy = AuthorizationPolicies.ManageSettings)]
    public async Task<IActionResult> Register([FromBody] RegisterInstanceCommand command, CancellationToken cancellationToken)
    {
        command.OrgId = GetOrgIdOrThrow();
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Get a Control Center instance by id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ViewSettings)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var orgId = GetOrgIdOrThrow();
        var query = new GetInstanceByIdQuery { Id = id, OrgId = orgId };
        var result = await _mediator.Send(query, cancellationToken);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Heartbeat from a Control Center instance.
    /// </summary>
    [HttpPost("{id:guid}/heartbeat")]
    [AllowAnonymous]
    public async Task<IActionResult> Heartbeat(Guid id, [FromBody] InstanceHeartbeatCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        // For device-initiated heartbeats without a full user token, you may want a separate auth mechanism.
        // For now, require orgId in body.
        if (command.OrgId == Guid.Empty)
        {
            return BadRequest(new { error = "OrgId is required." });
        }

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}

