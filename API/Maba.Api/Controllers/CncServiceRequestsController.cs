using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Cnc.DTOs;
using Maba.Application.Features.Cnc.Requests.Commands;
using Maba.Application.Features.Cnc.Requests.Queries;
using Maba.Domain.Cnc;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/cnc/requests")]
public class CncServiceRequestsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _environment;

    public CncServiceRequestsController(IMediator mediator, IWebHostEnvironment environment)
    {
        _mediator = mediator;
        _environment = environment;
    }

    /// <summary>
    /// Admin list: CNC service requests with filters and paging.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<CncServiceRequestsListResult>> GetRequests(
        [FromQuery] CncServiceRequestStatus? status = null,
        [FromQuery] string? serviceMode = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? createdFrom = null,
        [FromQuery] DateTime? createdTo = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 25)
    {
        var query = new GetAllCncServiceRequestsQuery
        {
            Status = status,
            ServiceMode = string.IsNullOrWhiteSpace(serviceMode) ? null : serviceMode.Trim(),
            SearchTerm = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            CreatedFromUtc = createdFrom,
            CreatedToUtc = createdTo,
            Skip = skip,
            Take = take
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get one request by id (admin).
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<CncServiceRequestDto>> GetById(Guid id)
    {
        var q = new GetCncServiceRequestByIdQuery { Id = id };
        var dto = await _mediator.Send(q);
        if (dto == null)
        {
            return NotFound();
        }

        return Ok(dto);
    }

    /// <summary>
    /// Download uploaded design / production file (admin).
    /// </summary>
    [HttpGet("{id:guid}/file")]
    [Authorize]
    public async Task<IActionResult> DownloadFile(Guid id)
    {
        var q = new GetCncServiceRequestByIdQuery { Id = id };
        var dto = await _mediator.Send(q);
        if (dto == null || string.IsNullOrWhiteSpace(dto.FilePath))
        {
            return NotFound();
        }

        var relative = dto.FilePath.TrimStart('/');
        var fullPath = Path.Combine(_environment.ContentRootPath, relative);
        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound();
        }

        var mime = GetMimeType(fullPath);
        var downloadName = string.IsNullOrWhiteSpace(dto.FileName)
            ? Path.GetFileName(fullPath)
            : dto.FileName;
        return PhysicalFile(fullPath, mime, downloadName);
    }

    /// <summary>
    /// Update status, admin notes, or prices (admin).
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<CncServiceRequestDto>> UpdateRequest(
        Guid id,
        [FromBody] UpdateCncServiceRequestCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    private static string GetMimeType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".zip" => "application/zip",
            ".gbr" => "application/octet-stream",
            ".gtl" or ".gbl" or ".gts" or ".gbs" or ".gto" or ".gbo" => "application/octet-stream",
            ".drl" => "application/octet-stream",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".svg" => "image/svg+xml",
            ".dxf" => "application/dxf",
            _ => "application/octet-stream"
        };
    }
}
