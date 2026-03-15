using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.AiChat.Messages.Commands;
using Maba.Application.Features.AiChat.DTOs;
using Maba.Application.Features.AiChat.Messages.Queries;
using GetAiMessagesBySessionQuery = Maba.Application.Features.AiChat.Messages.Queries.GetAiMessagesBySessionQuery;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AiMessagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AiMessagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("session/{sessionId}")]
    public async Task<ActionResult<List<AiMessageDto>>> GetAllAiMessages(Guid sessionId, [FromQuery] int? limit)
    {
        var query = new GetAiMessagesBySessionQuery { SessionId = sessionId, Limit = limit };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AiMessageDto>> CreateAiMessage([FromBody] CreateAiMessageCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAllAiMessages), new { sessionId = command.SessionId }, result);
    }
}

