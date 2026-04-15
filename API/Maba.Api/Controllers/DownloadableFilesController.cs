using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Downloads;

namespace Maba.Api.Controllers;

/// <summary>
/// Manages downloadable files (datasheets, manuals, certs, etc.) for any entity type.
/// Polymorphic: one controller handles Items, Projects, Machines, Software, etc.
/// </summary>
[ApiController]
[Route("api/v1/downloadable-files")]
public class DownloadableFilesController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<DownloadableFilesController> _logger;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".zip", ".rar", ".7z",
        ".png", ".jpg", ".jpeg", ".webp", ".svg",
        ".txt", ".csv",
        ".dwg", ".dxf", ".step", ".stp", ".iges", ".igs",
        ".stl", ".obj",
        ".bin", ".hex", ".firmware"
    };

    private const long MaxFileSizeBytes = 50L * 1024 * 1024; // 50 MB

    public DownloadableFilesController(
        IApplicationDbContext context,
        IFileStorageService fileStorage,
        ILogger<DownloadableFilesController> logger)
    {
        _context = context;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    // ─────────────────────────────────────────────
    // ADMIN ENDPOINTS (require Admin or Manager role)
    // ─────────────────────────────────────────────

    /// <summary>
    /// Upload a downloadable file and attach it to an entity.
    /// POST /api/v1/downloadable-files
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<DownloadableFileDto>> Upload(
        IFormFile file,
        [FromForm] string entityType,
        [FromForm] string entityId,
        [FromForm] string title,
        [FromForm] string? category,
        [FromForm] string? description,
        [FromForm] int sortOrder = 0,
        [FromForm] bool isFeatured = false,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        if (string.IsNullOrWhiteSpace(entityType))
            return BadRequest("entityType is required.");

        if (!Guid.TryParse(entityId, out var entityGuid))
            return BadRequest("entityId must be a valid GUID.");

        if (string.IsNullOrWhiteSpace(title))
            return BadRequest("title is required.");

        if (file.Length > MaxFileSizeBytes)
            return BadRequest($"File exceeds maximum allowed size of {MaxFileSizeBytes / 1024 / 1024} MB.");

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            return BadRequest($"File type '{ext}' is not allowed.");

        var safeCategory = DownloadCategory.All.Contains(category ?? "") ? category! : DownloadCategory.Other;
        var folder = $"downloads/{entityType.ToLowerInvariant()}";

        var safeName = SanitizeFileName(file.FileName);
        var storedName = $"{Guid.NewGuid()}_{safeName}";

        string storedPath;
        await using (var stream = file.OpenReadStream())
            storedPath = await _fileStorage.SaveFileAsync(stream, storedName, file.ContentType ?? "application/octet-stream", folder);

        var uploadedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var entity = new DownloadableFile
        {
            Id = Guid.NewGuid(),
            EntityType = entityType.Trim(),
            EntityId = entityGuid,
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Category = safeCategory,
            FileName = file.FileName,
            StoredPath = storedPath,
            ContentType = file.ContentType ?? "application/octet-stream",
            FileSizeBytes = file.Length,
            SortOrder = sortOrder,
            IsActive = true,
            IsFeatured = isFeatured,
            DownloadCount = 0,
            UploadedBy = uploadedBy
        };

        _context.Set<DownloadableFile>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("DownloadableFile {Id} uploaded for {EntityType}/{EntityId} by {User}",
            entity.Id, entityType, entityId, uploadedBy);

        return Ok(MapToDto(entity));
    }

    /// <summary>
    /// List all files for an entity (admin — includes inactive).
    /// GET /api/v1/downloadable-files?entityType=Item&entityId={id}
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IReadOnlyList<DownloadableFileDto>>> ListAdmin(
        [FromQuery] string entityType,
        [FromQuery] string entityId,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(entityId, out var entityGuid))
            return BadRequest("entityId must be a valid GUID.");

        var files = await _context.Set<DownloadableFile>()
            .Where(f => f.EntityType == entityType && f.EntityId == entityGuid)
            .OrderBy(f => f.SortOrder).ThenBy(f => f.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(files.Select(MapToDto).ToList());
    }

    /// <summary>
    /// Update metadata for a file (title, category, description, sortOrder, isActive, isFeatured).
    /// PUT /api/v1/downloadable-files/{id}
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<DownloadableFileDto>> Update(
        Guid id,
        [FromBody] UpdateDownloadableFileRequest request,
        CancellationToken cancellationToken = default)
    {
        var file = await _context.Set<DownloadableFile>().FindAsync([id], cancellationToken);
        if (file == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(request.Title))
            file.Title = request.Title.Trim();

        if (request.Description != null)
            file.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        if (!string.IsNullOrWhiteSpace(request.Category))
            file.Category = DownloadCategory.All.Contains(request.Category) ? request.Category : DownloadCategory.Other;

        if (request.SortOrder.HasValue)
            file.SortOrder = request.SortOrder.Value;

        if (request.IsActive.HasValue)
            file.IsActive = request.IsActive.Value;

        if (request.IsFeatured.HasValue)
            file.IsFeatured = request.IsFeatured.Value;

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(MapToDto(file));
    }

    /// <summary>
    /// Delete a file and remove it from storage.
    /// DELETE /api/v1/downloadable-files/{id}
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var file = await _context.Set<DownloadableFile>().FindAsync([id], cancellationToken);
        if (file == null) return NotFound();

        // Delete from file storage (best-effort)
        try { await _fileStorage.DeleteFileAsync(file.StoredPath); }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not delete stored file {Path}", file.StoredPath); }

        _context.Set<DownloadableFile>().Remove(file);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // ─────────────────────────────────────────────
    // PUBLIC ENDPOINTS (anonymous — active files only)
    // ─────────────────────────────────────────────

    /// <summary>
    /// List active files for an entity (public — only IsActive=true).
    /// GET /api/v1/downloadable-files/public?entityType=Item&entityId={id}
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<DownloadableFileDto>>> ListPublic(
        [FromQuery] string entityType,
        [FromQuery] string entityId,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(entityId, out var entityGuid))
            return BadRequest("entityId must be a valid GUID.");

        var files = await _context.Set<DownloadableFile>()
            .Where(f => f.EntityType == entityType && f.EntityId == entityGuid && f.IsActive)
            .OrderBy(f => f.SortOrder).ThenBy(f => f.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(files.Select(MapToDto).ToList());
    }

    /// <summary>
    /// Download a file. Increments DownloadCount. Active files only for anonymous users.
    /// GET /api/v1/downloadable-files/{id}/download
    /// </summary>
    [HttpGet("{id:guid}/download")]
    [AllowAnonymous]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken = default)
    {
        var file = await _context.Set<DownloadableFile>().FindAsync([id], cancellationToken);
        if (file == null) return NotFound();

        // Anonymous/public users can only download active files
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager");
        if (!file.IsActive && !isAdmin)
            return NotFound();

        var stream = await _fileStorage.GetFileAsync(file.StoredPath);
        if (stream == null)
            return NotFound("File not found in storage.");

        // Increment download count (fire-and-forget, don't fail on error)
        try
        {
            file.DownloadCount++;
            await _context.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not increment download count for {FileId}", id);
        }

        return File(stream, file.ContentType, file.FileName);
    }

    // ─────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────

    private static DownloadableFileDto MapToDto(DownloadableFile f) => new()
    {
        Id = f.Id,
        EntityType = f.EntityType,
        EntityId = f.EntityId,
        Title = f.Title,
        Description = f.Description,
        Category = f.Category,
        FileName = f.FileName,
        ContentType = f.ContentType,
        FileSizeBytes = f.FileSizeBytes,
        SortOrder = f.SortOrder,
        IsActive = f.IsActive,
        IsFeatured = f.IsFeatured,
        DownloadCount = f.DownloadCount,
        CreatedAt = f.CreatedAt
    };

    private static string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var safe = string.Concat(fileName.Select(c => invalid.Contains(c) ? '_' : c));
        return safe.Length > 100 ? safe[..100] : safe;
    }
}

// ─────────────────────────────────────────────
// DTOs
// ─────────────────────────────────────────────

public sealed class DownloadableFileDto
{
    public Guid Id { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public Guid EntityId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Category { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
    public bool IsFeatured { get; init; }
    public int DownloadCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class UpdateDownloadableFileRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? Category { get; init; }
    public int? SortOrder { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsFeatured { get; init; }
}
