using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Announcements;

namespace Maba.Api.Controllers;

// ─── DTOs ────────────────────────────────────────────────────────────────────

public class AppAnnouncementDto
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Type { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public string TargetPlatform { get; set; } = "Desktop";
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageAltText { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateAppAnnouncementRequest
{
    public string Message { get; set; } = string.Empty;
    public string? Type { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
    public string TargetPlatform { get; set; } = "Desktop";
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageAltText { get; set; }
}

public class UpdateAppAnnouncementRequest
{
    public string Message { get; set; } = string.Empty;
    public string? Type { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public string TargetPlatform { get; set; } = "Desktop";
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageAltText { get; set; }
}

// ─── Admin controller ─────────────────────────────────────────────────────────

[ApiController]
[Route("api/v1/app-announcements")]
[Authorize(Roles = "Admin,Manager")]
public class AppAnnouncementsController : ControllerBase
{
    private static readonly HashSet<string> AllowedTypes =
        new(StringComparer.OrdinalIgnoreCase) { "System", "Machine", "Module", "Catalog", "Info" };

    private static readonly HashSet<string> AllowedPlatforms =
        new(StringComparer.OrdinalIgnoreCase) { "All", "Desktop", "Web" };

    private readonly IApplicationDbContext _context;

    public AppAnnouncementsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("admin")]
    public async Task<ActionResult<List<AppAnnouncementDto>>> GetAdmin(CancellationToken cancellationToken)
    {
        var items = await _context.Set<AppAnnouncementItem>()
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(items.Select(ToDto).ToList());
    }

    [HttpPost("admin")]
    public async Task<ActionResult<AppAnnouncementDto>> Create(
        [FromBody] CreateAppAnnouncementRequest req,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateRequest(req.Message, req.Type, req.TargetPlatform, req.StartsAt, req.EndsAt);
        if (validationError != null) return BadRequest(new { message = validationError });

        var platform = string.IsNullOrWhiteSpace(req.TargetPlatform) ? "Desktop" : req.TargetPlatform.Trim();

        var entity = new AppAnnouncementItem
        {
            Message = req.Message.Trim(),
            Type = req.Type?.Trim(),
            IsActive = req.IsActive,
            DisplayOrder = req.DisplayOrder,
            TargetPlatform = platform,
            StartsAt = req.StartsAt,
            EndsAt = req.EndsAt,
            ImageUrl = string.IsNullOrWhiteSpace(req.ImageUrl) ? null : req.ImageUrl.Trim(),
            ImageAltText = string.IsNullOrWhiteSpace(req.ImageAltText) ? null : req.ImageAltText.Trim()
        };

        _context.Set<AppAnnouncementItem>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetAdmin), new { id = entity.Id }, ToDto(entity));
    }

    [HttpPut("admin/{id:guid}")]
    public async Task<ActionResult<AppAnnouncementDto>> Update(
        Guid id,
        [FromBody] UpdateAppAnnouncementRequest req,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateRequest(req.Message, req.Type, req.TargetPlatform, req.StartsAt, req.EndsAt);
        if (validationError != null) return BadRequest(new { message = validationError });

        var entity = await _context.Set<AppAnnouncementItem>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity == null) return NotFound();

        var platform = string.IsNullOrWhiteSpace(req.TargetPlatform) ? "Desktop" : req.TargetPlatform.Trim();

        entity.Message = req.Message.Trim();
        entity.Type = req.Type?.Trim();
        entity.IsActive = req.IsActive;
        entity.DisplayOrder = req.DisplayOrder;
        entity.TargetPlatform = platform;
        entity.StartsAt = req.StartsAt;
        entity.EndsAt = req.EndsAt;
        entity.ImageUrl = string.IsNullOrWhiteSpace(req.ImageUrl) ? null : req.ImageUrl.Trim();
        entity.ImageAltText = string.IsNullOrWhiteSpace(req.ImageAltText) ? null : req.ImageAltText.Trim();

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpDelete("admin/{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<AppAnnouncementItem>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity == null) return NotFound();

        _context.Set<AppAnnouncementItem>().Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("admin/{id:guid}/toggle")]
    public async Task<ActionResult<AppAnnouncementDto>> Toggle(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<AppAnnouncementItem>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity == null) return NotFound();

        entity.IsActive = !entity.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    private static string? ValidateRequest(string message, string? type, string platform, DateTime? startsAt, DateTime? endsAt)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "Message is required.";

        if (!string.IsNullOrWhiteSpace(type) && !AllowedTypes.Contains(type.Trim()))
            return $"Invalid type '{type}'. Allowed values: {string.Join(", ", AllowedTypes)}.";

        var normalizedPlatform = string.IsNullOrWhiteSpace(platform) ? "Desktop" : platform.Trim();
        if (!AllowedPlatforms.Contains(normalizedPlatform))
            return $"Invalid platform '{platform}'. Allowed values: {string.Join(", ", AllowedPlatforms)}.";

        if (startsAt.HasValue && endsAt.HasValue && startsAt.Value > endsAt.Value)
            return "StartsAt must be before or equal to EndsAt.";

        return null;
    }

    private static AppAnnouncementDto ToDto(AppAnnouncementItem e) => new()
    {
        Id = e.Id,
        Message = e.Message,
        Type = e.Type,
        IsActive = e.IsActive,
        DisplayOrder = e.DisplayOrder,
        TargetPlatform = e.TargetPlatform,
        StartsAt = e.StartsAt,
        EndsAt = e.EndsAt,
        ImageUrl = e.ImageUrl,
        ImageAltText = e.ImageAltText,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}

// ─── Control Center (desktop app) endpoint ───────────────────────────────────

[ApiController]
[Route("api/v1/control-center")]
[Authorize]
public class ControlCenterAnnouncementsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public ControlCenterAnnouncementsController(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns active, currently-valid announcements targeted at Desktop or All platforms,
    /// ordered by DisplayOrder then CreatedAt. Used by the WPF desktop app ticker.
    /// </summary>
    [HttpGet("announcements")]
    public async Task<ActionResult<List<AppAnnouncementDto>>> GetForDesktop(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var items = await _context.Set<AppAnnouncementItem>()
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                (x.TargetPlatform == "All" || x.TargetPlatform == "Desktop") &&
                (x.StartsAt == null || x.StartsAt <= now) &&
                (x.EndsAt == null || x.EndsAt >= now))
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(items.Select(a => new
        {
            a.Id,
            a.Message,
            a.Type,
            a.DisplayOrder,
            a.TargetPlatform,
            StartsAt = a.StartsAt,
            EndsAt = a.EndsAt,
            a.ImageUrl,
            a.ImageAltText
        }));
    }
}
