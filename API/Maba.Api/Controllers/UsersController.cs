using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Users.Commands;
using Maba.Application.Features.Users.DTOs;
using Maba.Application.Features.Users.Queries;
using Maba.Application.Common.Models;
using Maba.Application.Features.Auth.Commands;
using System.Security.Claims;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers([FromQuery] bool? isActive)
    {
        var query = new GetAllUsersQuery { IsActive = isActive };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id)
    {
        var query = new GetUserByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, result);
    }

    [HttpDelete("{id}")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        var command = new DeleteUserCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpGet("search")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<PagedResult<UserDto>>> SearchUsers(
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isActive,
        [FromQuery] Guid? roleId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new SearchUsersQuery
        {
            SearchTerm = searchTerm,
            IsActive = isActive,
            RoleId = roleId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("role/{roleId}")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<List<UserDto>>> GetUsersByRole(Guid roleId, [FromQuery] bool? isActive)
    {
        var query = new GetUsersByRoleQuery { RoleId = roleId, IsActive = isActive };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("{id}/email")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult> UpdateUserEmail(Guid id, [FromBody] UpdateEmailCommand command)
    {
        command.UserId = id;
        await _mediator.Send(command);
        return Ok(new { message = "Email updated successfully." });
    }

    [HttpPut("{id}/password")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult> UpdateUserPassword(Guid id, [FromBody] ChangePasswordCommand command)
    {
        command.UserId = id;
        await _mediator.Send(command);
        return Ok(new { message = "Password updated successfully." });
    }

    [HttpGet("{userId}/roles")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<List<Maba.Application.Features.Roles.DTOs.RoleDto>>> GetUserRoles(Guid userId)
    {
        var query = new Maba.Application.Features.Roles.Queries.GetUserRolesQuery { UserId = userId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("count")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<int>> GetUsersCount([FromQuery] bool? isActive)
    {
        var users = await _mediator.Send(new GetAllUsersQuery { IsActive = isActive });
        return Ok(users.Count);
    }
}

