using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using Maba.Application.Features.Laser.Requests.Commands;
using Maba.Application.Features.Laser.Requests.Queries;
using Maba.Application.Features.Laser.DTOs;
using Maba.Domain.Laser;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/laser/requests")]
public class LaserServiceRequestsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LaserServiceRequestsController> _logger;

    public LaserServiceRequestsController(
        IMediator mediator,
        IWebHostEnvironment environment,
        ILogger<LaserServiceRequestsController> logger)
    {
        _mediator = mediator;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Submit a new laser service request (public endpoint)
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<CreateLaserServiceRequestResultDto>> SubmitRequest(
        [FromForm] Guid materialId,
        [FromForm] string operationMode,
        [FromForm] IFormFile image,
        [FromForm] decimal? widthCm = null,
        [FromForm] decimal? heightCm = null,
        [FromForm] string? customerName = null,
        [FromForm] string? customerEmail = null,
        [FromForm] string? customerPhone = null,
        [FromForm] string? customerNotes = null)
    {
        if (image == null || image.Length == 0)
        {
            return BadRequest("An image is required for the laser service request.");
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".bmp", ".ai", ".eps", ".pdf" };
        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest($"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}");
        }

        const long maxFileSize = 20 * 1024 * 1024; // 20 MB
        if (image.Length > maxFileSize)
        {
            return BadRequest("File size exceeds the maximum allowed size of 20 MB.");
        }

        try
        {
            var uploadsFolder = Path.Combine(_environment.ContentRootPath, "uploads", "laser-requests");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/laser-requests/{uniqueFileName}";

            // Extract optional userId from JWT (works even on anonymous endpoints when a token is present)
            Guid? userId = null;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            var command = new CreateLaserServiceRequestCommand
            {
                MaterialId = materialId,
                OperationMode = operationMode,
                ImagePath = relativePath,
                ImageFileName = image.FileName,
                WidthCm = widthCm,
                HeightCm = heightCm,
                CustomerName = customerName,
                CustomerEmail = customerEmail,
                CustomerPhone = customerPhone,
                CustomerNotes = customerNotes,
                UserId = userId
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting laser service request");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Get all laser service requests (admin only)
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<LaserServiceRequestDto>>> GetAllRequests(
        [FromQuery] LaserServiceRequestStatus? status = null,
        [FromQuery] int? limit = null,
        [FromQuery] int? offset = null)
    {
        var query = new GetAllLaserServiceRequestsQuery
        {
            Status = status,
            Limit = limit,
            Offset = offset
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a laser service request by ID (admin only)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<LaserServiceRequestDto>> GetRequestById(Guid id)
    {
        var query = new GetLaserServiceRequestByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Update a laser service request (admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<LaserServiceRequestDto>> UpdateRequest(
        Guid id,
        [FromBody] UpdateLaserServiceRequestCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Get uploaded image for a request
    /// </summary>
    [HttpGet("{id}/image")]
    [Authorize]
    public async Task<IActionResult> GetRequestImage(Guid id)
    {
        var query = new GetLaserServiceRequestByIdQuery { Id = id };
        var request = await _mediator.Send(query);

        if (request == null)
        {
            return NotFound();
        }

        var imagePath = Path.Combine(_environment.ContentRootPath, request.ImagePath.TrimStart('/'));
        
        if (!System.IO.File.Exists(imagePath))
        {
            return NotFound("Image file not found.");
        }

        var mimeType = GetMimeType(imagePath);
        return PhysicalFile(imagePath, mimeType);
    }

    private static string GetMimeType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            ".bmp" => "image/bmp",
            ".ai" => "application/postscript",
            ".eps" => "application/postscript",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}
