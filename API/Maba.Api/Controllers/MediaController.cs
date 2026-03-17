using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Media.Commands;
using Maba.Application.Features.Media.DTOs;
using Maba.Application.Features.Media.Queries;
using System.Security.Claims;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/media")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMediaService _mediaService;

    public MediaController(IMediator mediator, IMediaService mediaService)
    {
        _mediator = mediator;
        _mediaService = mediaService;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<MediaAssetDto>> UploadMedia([FromForm] UploadMediaCommand command)
    {
        // Set uploaded by user ID from claims if not provided
        if (!command.UploadedByUserId.HasValue)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                command.UploadedByUserId = userId;
            }
        }

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetMediaById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<List<MediaAssetDto>>> GetAllMedia([FromQuery] Guid? mediaTypeId, [FromQuery] Guid? uploadedByUserId)
    {
        var query = new GetAllMediaQuery
        {
            MediaTypeId = mediaTypeId,
            UploadedByUserId = uploadedByUserId
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MediaAssetDto>> GetMediaById(Guid id)
    {
        var query = new GetMediaByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Returns the binary content of a media asset by id.
    /// This endpoint can be used by the frontend everywhere instead of static file URLs.
    /// </summary>
    [HttpGet("{id:guid}/content")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMediaContent(Guid id, CancellationToken cancellationToken)
    {
        var streamResult = await _mediaService.GetStreamAsync(id, cancellationToken);
        if (streamResult is null)
        {
            return NotFound();
        }

        // TODO: Add finer-grained authorization for non-public media (owner/admin/etc.).
        if (!streamResult.IsPublic && !User.Identity?.IsAuthenticated == true)
        {
            return Unauthorized();
        }

        return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MediaAssetDto>> UpdateMedia(Guid id, [FromBody] UpdateMediaCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMedia(Guid id)
    {
        var command = new DeleteMediaCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}

