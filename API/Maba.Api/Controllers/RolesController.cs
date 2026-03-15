using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Roles.Commands;
using Maba.Application.Features.Roles.DTOs;
using Maba.Application.Features.Roles.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<RoleDto>>> GetAllRoles()
    {
        var query = new GetAllRolesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDto>> GetRoleById(Guid id)
    {
        var query = new GetRoleByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetRoleById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RoleDto>> UpdateRole(Guid id, [FromBody] UpdateRoleCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRole(Guid id)
    {
        var command = new DeleteRoleCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("{roleId}/users/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> AssignRoleToUser(Guid roleId, Guid userId)
    {
        var command = new AssignRoleToUserCommand { RoleId = roleId, UserId = userId };
        await _mediator.Send(command);
        return Ok(new { message = "Role assigned successfully." });
    }

    [HttpDelete("{roleId}/users/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> RemoveRoleFromUser(Guid roleId, Guid userId)
    {
        var command = new RemoveRoleFromUserCommand { RoleId = roleId, UserId = userId };
        await _mediator.Send(command);
        return Ok(new { message = "Role removed successfully." });
    }

    [HttpPost("{roleId}/permissions/{permissionId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> AssignPermissionToRole(Guid roleId, Guid permissionId)
    {
        var command = new AssignPermissionToRoleCommand { RoleId = roleId, PermissionId = permissionId };
        await _mediator.Send(command);
        return Ok(new { message = "Permission assigned successfully." });
    }

    [HttpDelete("{roleId}/permissions/{permissionId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> RemovePermissionFromRole(Guid roleId, Guid permissionId)
    {
        var command = new RemovePermissionFromRoleCommand { RoleId = roleId, PermissionId = permissionId };
        await _mediator.Send(command);
        return Ok(new { message = "Permission removed successfully." });
    }

    [HttpGet("{roleId}/permissions")]
    public async Task<ActionResult<List<PermissionDto>>> GetRolePermissions(Guid roleId)
    {
        var query = new GetRolePermissionsQuery { RoleId = roleId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

