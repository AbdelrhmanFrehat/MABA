using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.Text.Json;
using Maba.Application.Common.Interfaces;
using Maba.Domain.ControlCenter;

namespace Maba.Api.Controllers;

// ─── DTOs ────────────────────────────────────────────────────────────────────

public class DesktopUpdateManifest
{
    public string Version { get; set; } = string.Empty;
    public string PackageUri { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
}

public class DesktopChannelInfo
{
    public string Channel { get; set; } = string.Empty;
    public DesktopUpdateManifest? LatestManifest { get; set; }
    public string ManifestUrl { get; set; } = string.Empty;
    public List<string> Packages { get; set; } = new();
}

public class AppRuntimeMetadataDto
{
    public Guid Id { get; set; }
    public string Channel { get; set; } = "stable";
    public string AppVersion { get; set; } = string.Empty;
    public string FirmwareName { get; set; } = string.Empty;
    public string FirmwareVersion { get; set; } = string.Empty;
    public string TargetBoard { get; set; } = string.Empty;
    public string ProtocolName { get; set; } = string.Empty;
    public string? CommandSummary { get; set; }
    public string? CompatibilityNotes { get; set; }
    public bool IsActive { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpsertRuntimeMetadataRequest
{
    public string Channel { get; set; } = "stable";
    public string AppVersion { get; set; } = string.Empty;
    public string FirmwareName { get; set; } = string.Empty;
    public string FirmwareVersion { get; set; } = string.Empty;
    public string TargetBoard { get; set; } = string.Empty;
    public string ProtocolName { get; set; } = string.Empty;
    public string? CommandSummary { get; set; }
    public string? CompatibilityNotes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? PublishedAt { get; set; }
}

// ─── Controller ──────────────────────────────────────────────────────────────

/// <summary>
/// Hosts the desktop app update feed.
///
/// Public endpoints (no auth):
///   GET /desktop-updates/{channel}/manifest.json  → served as static file by UseStaticFiles
///   GET /desktop-updates/{channel}/{filename}      → served as static file by UseStaticFiles
///
/// Admin endpoints:
///   POST /api/v1/desktop-updates/publish           → upload new zip + update manifest
///   GET  /api/v1/desktop-updates/channels          → list available channels + current versions
/// </summary>
[ApiController]
[Route("api/v1/desktop-updates")]
public class DesktopUpdatesController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DesktopUpdatesController> _logger;

    private static readonly JsonSerializerOptions _json = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public DesktopUpdatesController(
        IWebHostEnvironment env,
        IApplicationDbContext context,
        ILogger<DesktopUpdatesController> logger)
    {
        _env = env;
        _context = context;
        _logger = logger;
    }

    // ── Admin: publish a new release (streaming — no double-copy) ────────────

    /// <summary>
    /// Upload a new desktop release zip and update the channel manifest.
    ///
    /// OPTIMISED: uses MultipartReader to stream the zip DIRECTLY from the network
    /// to the final wwwroot path. No temp file, no double disk write.
    /// Fields must arrive before the file section (standard curl / browser form order).
    ///
    /// Form fields: version*, channel (stable|beta), notes, file* (.zip)
    /// </summary>
    [HttpPost("publish")]
    [Authorize(Roles = "Admin,Manager")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue, ValueLengthLimit = int.MaxValue)]
    public async Task<ActionResult<DesktopUpdateManifest>> Publish(CancellationToken cancellationToken = default)
    {
        if (!Request.HasFormContentType ||
            !MediaTypeHeaderValue.TryParse(Request.ContentType, out var mt) ||
            !mt.MediaType.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Request must be multipart/form-data." });
        }

        var boundary = HeaderUtilities.RemoveQuotes(mt.Boundary).Value;
        if (string.IsNullOrWhiteSpace(boundary))
            return BadRequest(new { message = "Missing multipart boundary." });

        var reader = new MultipartReader(boundary, Request.Body);
        // Use a generous section body length limit — we stream to disk so memory usage is negligible
        reader.BodyLengthLimit = null;

        string? version = null, channel = "stable", notes = null;
        DesktopUpdateManifest? result = null;

        var section = await reader.ReadNextSectionAsync(cancellationToken);
        while (section != null)
        {
            if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var disp))
            {
                section = await reader.ReadNextSectionAsync(cancellationToken);
                continue;
            }

            if (disp.IsFormDisposition())
            {
                // Small text field — read entirely into string (safe, tiny)
                using var sr = new StreamReader(section.Body, leaveOpen: false);
                var value = (await sr.ReadToEndAsync(cancellationToken)).Trim();
                var name = HeaderUtilities.RemoveQuotes(disp.Name).Value?.ToLowerInvariant();
                switch (name)
                {
                    case "version": version = value.TrimStart('v'); break;
                    case "channel": channel = value == "beta" ? "beta" : "stable"; break;
                    case "notes":   notes = value; break;
                }
            }
            else if (disp.IsFileDisposition())
            {
                // ── Validate pre-requisites ──
                if (string.IsNullOrWhiteSpace(version))
                    return BadRequest(new { message = "version field must appear before the file part." });

                var fileName = Path.GetFileName(
                    HeaderUtilities.RemoveQuotes(disp.FileName).Value?.Trim() ?? "package.zip");

                if (!fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { message = "Only .zip packages are supported." });

                // ── Stream directly to final path — NO temp file, NO double copy ──
                var channelDir = Path.Combine(_env.WebRootPath, "desktop-updates", channel);
                Directory.CreateDirectory(channelDir);

                var finalPath = Path.Combine(channelDir, fileName);
                var tmpZip   = finalPath + ".uploading";

                try
                {
                    // 64 KB async buffer — efficient for large sequential writes
                    await using (var fs = new FileStream(
                        tmpZip, FileMode.Create, FileAccess.Write,
                        FileShare.None, bufferSize: 65536, useAsync: true))
                    {
                        await section.Body.CopyToAsync(fs, bufferSize: 65536, cancellationToken);
                    }

                    // Atomic replace of old zip (avoids partial-file reads by desktop updater)
                    System.IO.File.Move(tmpZip, finalPath, overwrite: true);
                }
                catch
                {
                    if (System.IO.File.Exists(tmpZip))
                        System.IO.File.Delete(tmpZip);
                    throw;
                }

                // ── Write manifest atomically via temp+rename ──
                var manifest = new DesktopUpdateManifest
                {
                    Version    = version!,
                    PackageUri = fileName,
                    Notes      = string.IsNullOrWhiteSpace(notes) ? $"Release {version}" : notes,
                    PublishedAt = DateTime.UtcNow
                };
                var manifestPath = Path.Combine(channelDir, "manifest.json");
                var tmpManifest  = manifestPath + ".tmp";
                await System.IO.File.WriteAllTextAsync(
                    tmpManifest, JsonSerializer.Serialize(manifest, _json), cancellationToken);
                System.IO.File.Move(tmpManifest, manifestPath, overwrite: true);

                _logger.LogInformation(
                    "Desktop update published (streamed): channel={Channel} v={Version} file={File}",
                    channel, version, fileName);

                result = manifest;
            }

            section = await reader.ReadNextSectionAsync(cancellationToken);
        }

        if (result == null)
            return BadRequest(new { message = "No file section found in multipart body." });

        return Ok(result);
    }

    // ── Admin: list channels ──────────────────────────────────────────────────

    [HttpGet("channels")]
    [Authorize(Roles = "Admin,Manager")]
    public ActionResult<List<DesktopChannelInfo>> GetChannels()
    {
        var baseDir = Path.Combine(_env.WebRootPath, "desktop-updates");
        if (!Directory.Exists(baseDir))
            return Ok(new List<DesktopChannelInfo>());

        var request = HttpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";

        var result = new List<DesktopChannelInfo>();
        foreach (var channelDir in Directory.GetDirectories(baseDir))
        {
            var channelName = Path.GetFileName(channelDir);
            var manifestPath = Path.Combine(channelDir, "manifest.json");
            DesktopUpdateManifest? manifest = null;

            if (System.IO.File.Exists(manifestPath))
            {
                try
                {
                    var json = System.IO.File.ReadAllText(manifestPath);
                    manifest = JsonSerializer.Deserialize<DesktopUpdateManifest>(json, _json);
                }
                catch { /* ignore malformed manifest */ }
            }

            var packages = Directory.GetFiles(channelDir, "*.zip")
                .Select(Path.GetFileName)
                .Where(f => f != null)
                .Cast<string>()
                .OrderByDescending(f => f)
                .ToList();

            result.Add(new DesktopChannelInfo
            {
                Channel = channelName,
                LatestManifest = manifest,
                ManifestUrl = $"{baseUrl}/desktop-updates/{channelName}/manifest.json",
                Packages = packages
            });
        }

        return Ok(result);
    }

    // ── Admin: delete a specific package ─────────────────────────────────────

    [HttpDelete("channels/{channel}/packages/{fileName}")]
    [Authorize(Roles = "Admin,Manager")]
    public IActionResult DeletePackage(string channel, string fileName)
    {
        channel = channel.Trim().ToLowerInvariant();
        fileName = Path.GetFileName(fileName.Trim()); // prevent path traversal

        if (!fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Only .zip files can be deleted." });

        var filePath = Path.Combine(_env.WebRootPath, "desktop-updates", channel, fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound();

        System.IO.File.Delete(filePath);
        _logger.LogInformation("Desktop package deleted: {Channel}/{File}", channel, fileName);
        return NoContent();
    }

    // ── Runtime metadata: CRUD ────────────────────────────────────────────────

    [HttpGet("runtime-metadata")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<List<AppRuntimeMetadataDto>>> GetRuntimeMetadata(CancellationToken ct)
    {
        var list = await _context.Set<AppRuntimeMetadata>()
            .AsNoTracking()
            .OrderByDescending(x => x.PublishedAt ?? x.CreatedAt)
            .ToListAsync(ct);
        return Ok(list.Select(ToDto).ToList());
    }

    [HttpGet("runtime-metadata/current/{channel}")]
    [AllowAnonymous]
    public async Task<ActionResult<AppRuntimeMetadataDto>> GetCurrentRuntimeMetadata(
        string channel = "stable", CancellationToken ct = default)
    {
        var entry = await _context.Set<AppRuntimeMetadata>()
            .AsNoTracking()
            .Where(x => x.Channel == channel && x.IsActive)
            .OrderByDescending(x => x.PublishedAt ?? x.CreatedAt)
            .FirstOrDefaultAsync(ct);
        if (entry == null) return NotFound();
        return Ok(ToDto(entry));
    }

    [HttpPost("runtime-metadata")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<AppRuntimeMetadataDto>> CreateRuntimeMetadata(
        [FromBody] UpsertRuntimeMetadataRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.AppVersion))
            return BadRequest(new { message = "appVersion is required." });

        var entity = new AppRuntimeMetadata
        {
            Channel = string.IsNullOrWhiteSpace(req.Channel) ? "stable" : req.Channel.Trim().ToLowerInvariant(),
            AppVersion = req.AppVersion.Trim().TrimStart('v'),
            FirmwareName = req.FirmwareName?.Trim() ?? string.Empty,
            FirmwareVersion = req.FirmwareVersion?.Trim().TrimStart('v') ?? string.Empty,
            TargetBoard = req.TargetBoard?.Trim() ?? string.Empty,
            ProtocolName = req.ProtocolName?.Trim() ?? string.Empty,
            CommandSummary = req.CommandSummary?.Trim(),
            CompatibilityNotes = req.CompatibilityNotes?.Trim(),
            IsActive = req.IsActive,
            PublishedAt = req.PublishedAt ?? DateTime.UtcNow
        };
        _context.Set<AppRuntimeMetadata>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return Ok(ToDto(entity));
    }

    [HttpPut("runtime-metadata/{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<AppRuntimeMetadataDto>> UpdateRuntimeMetadata(
        Guid id, [FromBody] UpsertRuntimeMetadataRequest req, CancellationToken ct)
    {
        var entity = await _context.Set<AppRuntimeMetadata>().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null) return NotFound();

        entity.Channel = string.IsNullOrWhiteSpace(req.Channel) ? "stable" : req.Channel.Trim().ToLowerInvariant();
        entity.AppVersion = req.AppVersion?.Trim().TrimStart('v') ?? entity.AppVersion;
        entity.FirmwareName = req.FirmwareName?.Trim() ?? entity.FirmwareName;
        entity.FirmwareVersion = req.FirmwareVersion?.Trim().TrimStart('v') ?? entity.FirmwareVersion;
        entity.TargetBoard = req.TargetBoard?.Trim() ?? entity.TargetBoard;
        entity.ProtocolName = req.ProtocolName?.Trim() ?? entity.ProtocolName;
        entity.CommandSummary = req.CommandSummary?.Trim();
        entity.CompatibilityNotes = req.CompatibilityNotes?.Trim();
        entity.IsActive = req.IsActive;
        entity.PublishedAt = req.PublishedAt;

        await _context.SaveChangesAsync(ct);
        return Ok(ToDto(entity));
    }

    [HttpDelete("runtime-metadata/{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> DeleteRuntimeMetadata(Guid id, CancellationToken ct)
    {
        var entity = await _context.Set<AppRuntimeMetadata>().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null) return NotFound();
        _context.Set<AppRuntimeMetadata>().Remove(entity);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    private static AppRuntimeMetadataDto ToDto(AppRuntimeMetadata e) => new()
    {
        Id = e.Id, Channel = e.Channel,
        AppVersion = e.AppVersion, FirmwareName = e.FirmwareName,
        FirmwareVersion = e.FirmwareVersion, TargetBoard = e.TargetBoard,
        ProtocolName = e.ProtocolName, CommandSummary = e.CommandSummary,
        CompatibilityNotes = e.CompatibilityNotes, IsActive = e.IsActive,
        PublishedAt = e.PublishedAt, CreatedAt = e.CreatedAt
    };
}
