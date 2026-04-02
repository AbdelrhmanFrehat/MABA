using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Features.Printing.Designs.Commands;
using Maba.Application.Features.Printing.DTOs;
using Maba.Application.Features.Printing.Designs.Queries;
using Maba.Application.Common.Models;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Media;
using System.Security.Claims;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DesignsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileStorageService _fileStorageService;
    private readonly IApplicationDbContext _context;

    public DesignsController(IMediator mediator, IFileStorageService fileStorageService, IApplicationDbContext context)
    {
        _mediator = mediator;
        _fileStorageService = fileStorageService;
        _context = context;
    }

    /// <summary>Current user's designs only. User id is taken from the JWT — never from query parameters.</summary>
    [HttpGet]
    public async Task<ActionResult<List<DesignDto>>> GetMyDesigns()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var query = new GetAllDesignsQuery { UserId = userId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<DesignDto>> UploadDesign(IFormFile file, [FromForm] string? title, [FromForm] string? notes)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        // Get current user ID from claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("User not authenticated");
        }

        // Generate file name and save
        var fileExtension = Path.GetExtension(file.FileName);
        var designTitle = title ?? Path.GetFileNameWithoutExtension(file.FileName);
        var savedFileName = $"{Guid.NewGuid()}{fileExtension}";
        
        string filePath;
        await using (var stream = file.OpenReadStream())
        {
            filePath = await _fileStorageService.SaveFileAsync(stream, savedFileName, file.ContentType, "designs");
        }

        var fileUrl = await _fileStorageService.GetFileUrlAsync(filePath);

        // Get or create a MediaType for 3D files
        var mediaType = await _context.Set<MediaType>()
            .FirstOrDefaultAsync(mt => mt.Key == "3D_MODEL") 
            ?? await _context.Set<MediaType>().FirstAsync();

        // Create MediaAsset
        var mediaAsset = new MediaAsset
        {
            Id = Guid.NewGuid(),
            FileUrl = fileUrl,
            MimeType = file.ContentType,
            FileName = file.FileName,
            FileExtension = fileExtension.TrimStart('.'),
            FileSizeBytes = file.Length,
            StorageProvider = "Local",
            StorageKey = filePath,
            IsPublic = false,
            UploadedByUserId = userId,
            MediaTypeId = mediaType.Id
        };
        _context.Set<MediaAsset>().Add(mediaAsset);

        // Create design using command
        var command = new CreateDesignCommand
        {
            UserId = userId,
            Title = designTitle,
            Notes = notes
        };
        var design = await _mediator.Send(command);

        // Add design file linked to MediaAsset
        var designFile = new Maba.Domain.Printing.DesignFile
        {
            Id = Guid.NewGuid(),
            DesignId = design.Id,
            MediaAssetId = mediaAsset.Id,
            Format = fileExtension.TrimStart('.').ToUpperInvariant(),
            FileSizeBytes = file.Length,
            IsPrimary = true,
            UploadedAt = DateTime.UtcNow
        };

        _context.Set<Maba.Domain.Printing.DesignFile>().Add(designFile);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Add file to response
        design.Files = new List<DesignFileDto>
        {
            new DesignFileDto
            {
                Id = designFile.Id,
                DesignId = design.Id,
                MediaAssetId = designFile.MediaAssetId,
                FileUrl = fileUrl,
                OriginalFileUrl = fileUrl,
                PreviewModelUrl = IsPreviewableFormat(designFile.Format) ? fileUrl : null,
                PreviewFormat = IsPreviewableFormat(designFile.Format) ? designFile.Format : null,
                ThumbnailUrl = mediaAsset.ThumbnailUrl,
                FileType = designFile.Format,
                IsPreviewable = IsPreviewableFormat(designFile.Format),
                FileName = file.FileName,
                Format = designFile.Format,
                FileSizeBytes = designFile.FileSizeBytes,
                IsPrimary = true,
                UploadedAt = designFile.UploadedAt,
                CreatedAt = designFile.CreatedAt
            }
        };

        return CreatedAtAction(nameof(GetMyDesigns), new { }, design);
    }

    private static bool IsPreviewableFormat(string? format)
    {
        var value = (format ?? string.Empty).Trim().ToUpperInvariant();
        return value is "GLB" or "GLTF" or "STL" or "OBJ";
    }

    [HttpPost("json")]
    [Consumes("application/json")]
    public async Task<ActionResult<DesignDto>> CreateDesign([FromBody] CreateDesignCommand command)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();
        command.UserId = userId;
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetMyDesigns), new { }, result);
    }

    [HttpGet("{id}/detail")]
    [AllowAnonymous]
    public async Task<ActionResult<DesignDetailDto>> GetDesignDetail(Guid id)
    {
        var query = new GetDesignDetailQuery { DesignId = id };
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var uid))
            query.RequestingUserId = uid;
        query.IsPrivileged = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("StoreOwner");
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}/files")]
    [AllowAnonymous]
    public async Task<ActionResult<List<DesignFileDto>>> GetDesignFiles(Guid id)
    {
        var query = new GetDesignFilesQuery { DesignId = id };
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var uid))
            query.RequestingUserId = uid;
        query.IsPrivileged = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("StoreOwner");
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<DesignDto>>> SearchDesigns(
        [FromQuery] string? searchTerm,
        [FromQuery] Guid? userId,
        [FromQuery] bool? isPublic,
        [FromQuery] string? licenseType,
        [FromQuery] string? tags,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true)
    {
        Guid? requestingUserId = null;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var uid))
            requestingUserId = uid;
        var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("StoreOwner");

        var query = new SearchDesignsQuery
        {
            SearchTerm = searchTerm,
            UserId = userId,
            IsPublic = isPublic,
            LicenseType = licenseType,
            Tags = tags,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDescending = sortDescending,
            RequestingUserId = requestingUserId,
            IsPrivileged = isPrivileged
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDesign(Guid id)
    {
        return await DeleteDesignInternal(id);
    }

    // Fallback endpoint for environments where DELETE preflight is blocked by proxy/WAF.
    [HttpPost("{id}/delete")]
    public async Task<IActionResult> DeleteDesignPost(Guid id)
    {
        return await DeleteDesignInternal(id);
    }

    private async Task<IActionResult> DeleteDesignInternal(Guid id)
    {
        var design = await _context.Set<Maba.Domain.Printing.Design>()
            .Include(d => d.DesignFiles)
            .ThenInclude(df => df.MediaAsset)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (design == null)
            return NotFound();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isOwner = Guid.TryParse(userIdClaim, out var currentUserId) && design.UserId == currentUserId;
        var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("StoreOwner");

        if (!isOwner && !isPrivileged)
            return Forbid();

        // Restrict delete when design files are referenced by jobs for normal users.
        // Privileged users can force-clean test data by removing dependent jobs first.
        var designFileIds = design.DesignFiles.Select(df => df.Id).ToList();
        if (designFileIds.Count > 0)
        {
            var slicingJobs = await _context.Set<Maba.Domain.Printing.SlicingJob>()
                .Where(sj => designFileIds.Contains(sj.DesignFileId))
                .ToListAsync();

            if (slicingJobs.Count > 0)
            {
                if (!isPrivileged)
                {
                    return BadRequest(new
                    {
                        message = "This design is linked to jobs and cannot be deleted from this account."
                    });
                }

                var slicingJobIds = slicingJobs.Select(sj => sj.Id).ToList();
                if (slicingJobIds.Count > 0)
                {
                    var printJobs = await _context.Set<Maba.Domain.Printing.PrintJob>()
                        .Where(pj => slicingJobIds.Contains(pj.SlicingJobId))
                        .ToListAsync();
                    if (printJobs.Count > 0)
                    {
                        _context.Set<Maba.Domain.Printing.PrintJob>().RemoveRange(printJobs);
                    }
                }

                _context.Set<Maba.Domain.Printing.SlicingJob>().RemoveRange(slicingJobs);
            }
        }

        foreach (var file in design.DesignFiles)
        {
            if (!string.IsNullOrWhiteSpace(file.MediaAsset?.StorageKey))
            {
                await _fileStorageService.DeleteFileAsync(file.MediaAsset.StorageKey);
            }
        }

        var mediaAssets = design.DesignFiles
            .Where(df => df.MediaAsset != null)
            .Select(df => df.MediaAsset!)
            .ToList();

        _context.Set<Maba.Domain.Printing.DesignFile>().RemoveRange(design.DesignFiles);
        if (mediaAssets.Count > 0)
        {
            _context.Set<MediaAsset>().RemoveRange(mediaAssets);
        }
        _context.Set<Maba.Domain.Printing.Design>().Remove(design);

        await _context.SaveChangesAsync(CancellationToken.None);
        return NoContent();
    }
}

