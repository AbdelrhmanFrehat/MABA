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
    public string TargetPlatform { get; set; } = "All";
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateAppAnnouncementRequest
{
    public string Message { get; set; } = string.Empty;
    public string? Type { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
    public string TargetPlatform { get; set; } = "All";
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
}

public class UpdateAppAnnouncementRequest
{
    public string Message { get; set; } = string.Empty;
    public string? Type { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public string TargetPlatform { get; set; } = "All";
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
}

// ─── Admin controller ─────────────────────────────────────────────────────────

[ApiController]
[Route("api/v1/app-announcements")]
[Authorize(Roles = "Admin,Manager")]
public class AppAnnouncementsController : ControllerBase
{
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
        if (string.IsNullOrWhiteSpace(req.Message))
            return BadRequest(new { message = "Message is required." });

        var entity = new AppAnnouncementItem
        {
            Message = req.Message.Trim(),
            Type = req.Type?.Trim(),
            IsActive = req.IsActive,
            DisplayOrder = req.DisplayOrder,
            TargetPlatform = string.IsNullOrWhiteSpace(req.TargetPlatform) ? "All" : req.TargetPlatform.Trim(),
            StartsAt = req.StartsAt,
            EndsAt = req.EndsAt
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
        if (string.IsNullOrWhiteSpace(req.Message))
            return BadRequest(new { message = "Message is required." });

        var entity = await _context.Set<AppAnnouncementItem>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity == null) return NotFound();

        entity.Message = req.Message.Trim();
        entity.Type = req.Type?.Trim();
        entity.IsActive = req.IsActive;
        entity.DisplayOrder = req.DisplayOrder;
        entity.TargetPlatform = string.IsNullOrWhiteSpace(req.TargetPlatform) ? "All" : req.TargetPlatform.Trim();
        entity.StartsAt = req.StartsAt;
        entity.EndsAt = req.EndsAt;

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
            a.DisplayOrder
        }));
    }
}
