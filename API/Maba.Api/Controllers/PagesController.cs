using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Cms.Pages.Commands;
using Maba.Application.Features.Cms.DTOs;
using Maba.Application.Features.Cms.Pages.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<PageDto>>> GetAllPages([FromQuery] bool? isActive)
    {
        var query = new GetAllPagesQuery { IsActive = isActive };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<PageDto>> CreatePage([FromBody] CreatePageCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAllPages), new { }, result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<PageDetailDto>> GetPageDetail(Guid id)
    {
        var query = new GetPageDetailQuery { PageId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("key/{key}")]
    [AllowAnonymous]
    public async Task<ActionResult<PageDto>> GetPageByKey(string key)
    {
        var query = new GetPageByKeyQuery { Key = key };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<PageDto>> GetPageBySlug(string slug)
    {
        var query = new GetPageByKeyQuery { Key = slug };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}/preview")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<PagePreviewDto>> GetPagePreview(Guid id)
    {
        var query = new GetPagePreviewQuery { PageId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<PageDto>> UpdatePage(Guid id, [FromBody] UpdatePageCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeletePage(Guid id)
    {
        var command = new DeletePageCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("{id}/publish")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<PageDto>> PublishPage(Guid id, [FromBody] PublishPageCommand command)
    {
        command.PageId = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{id}/unpublish")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<PageDto>> UnpublishPage(Guid id, [FromBody] UnpublishPageCommand command)
    {
        command.PageId = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

