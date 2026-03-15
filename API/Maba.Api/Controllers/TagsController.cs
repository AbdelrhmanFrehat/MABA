using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Catalog.Tags.Commands;
using Maba.Application.Features.Catalog.Tags.DTOs;
using Maba.Application.Features.Catalog.Tags.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TagsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<TagDto>>> GetAllTags([FromQuery] bool? isActive)
    {
        var query = new GetAllTagsQuery { IsActive = isActive };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<TagDto>> GetTagById(Guid id)
    {
        var query = new GetTagByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult<TagDto>> CreateTag([FromBody] CreateTagCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTagById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult<TagDto>> UpdateTag(Guid id, [FromBody] UpdateTagCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult> DeleteTag(Guid id)
    {
        var command = new DeleteTagCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<List<TagDto>>> SearchTags(
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isActive)
    {
        var query = new SearchTagsQuery { SearchTerm = searchTerm, IsActive = isActive };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("popular")]
    [AllowAnonymous]
    public async Task<ActionResult<List<TagDto>>> GetPopularTags([FromQuery] int? limit)
    {
        var query = new GetPopularTagsQuery { Limit = limit };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

