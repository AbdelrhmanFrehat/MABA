using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.AiChat.Sessions.Commands;
using Maba.Application.Features.AiChat.DTOs;
using Maba.Application.Features.AiChat.Sessions.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AiSessionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AiSessionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<AiSessionDto>>> GetAllAiSessions([FromQuery] Guid? userId)
    {
        var query = new GetAllAiSessionsQuery { UserId = userId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}/detail")]
    [AllowAnonymous]
    public async Task<ActionResult<AiSessionDetailDto>> GetAiSessionDetail(Guid id)
    {
        var query = new GetAiSessionDetailQuery { SessionId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<AiSessionDto>>> GetAiSessionsByUser(Guid userId, [FromQuery] bool? isActive)
    {
        var query = new GetAiSessionsByUserQuery { UserId = userId, IsActive = isActive };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AiSessionDto>> CreateAiSession([FromBody] CreateAiSessionCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAiSessionDetail), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AiSessionDto>> UpdateAiSession(Guid id, [FromBody] UpdateAiSessionCommand command)
    {
        command.SessionId = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{id}/end")]
    public async Task<ActionResult<AiSessionDto>> EndAiSession(Guid id)
    {
        var command = new EndAiSessionCommand { SessionId = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
