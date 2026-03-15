using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Cms.PageSections.Commands;
using Maba.Application.Features.Cms.DTOs;
using Maba.Application.Features.Cms.PageSections.Queries;
using System.Security.Claims;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PageSectionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PageSectionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("drafts")]
    public async Task<ActionResult<PageSectionDraftDto>> CreatePageSectionDraft([FromBody] CreatePageSectionDraftCommand command)
    {
        // Set created by user ID from claims if not provided
        if (!command.CreatedByUserId.HasValue)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                command.CreatedByUserId = userId;
            }
        }

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("drafts/page/{pageId}")]
    public async Task<ActionResult<List<PageSectionDraftDto>>> GetPageSectionsDraft(Guid pageId)
    {
        var query = new GetPageSectionsDraftQuery { PageId = pageId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("publish")]
    public async Task<ActionResult<PageSectionPublishedDto>> PublishPageSection([FromBody] PublishPageSectionCommand command)
    {
        // Set published by user ID from claims if not provided
        if (!command.PublishedByUserId.HasValue)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                command.PublishedByUserId = userId;
            }
        }

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("published/page/{pageId}")]
    public async Task<ActionResult<List<PageSectionPublishedDto>>> GetPageSectionsPublished(Guid pageId)
    {
        var query = new GetPageSectionsPublishedQuery { PageId = pageId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

