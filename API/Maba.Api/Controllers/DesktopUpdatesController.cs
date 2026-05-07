using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
    private readonly ILogger<DesktopUpdatesController> _logger;

    private static readonly JsonSerializerOptions _json = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public DesktopUpdatesController(IWebHostEnvironment env, ILogger<DesktopUpdatesController> logger)
    {
        _env = env;
        _logger = logger;
    }

    // ── Admin: publish a new release ─────────────────────────────────────────

    /// <summary>
    /// Upload a new desktop release zip and update the channel manifest.
    /// Form fields: file (zip), version, notes, channel (default: stable)
    /// </summary>
    [HttpPost("publish")]
    [Authorize(Roles = "Admin,Manager")]
    [Consumes("multipart/form-data")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue, ValueLengthLimit = int.MaxValue)]
    public async Task<ActionResult<DesktopUpdateManifest>> Publish(
        IFormFile file,
        [FromForm] string version,
        [FromForm] string? notes,
        [FromForm] string channel = "stable",
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided." });

        if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Only .zip packages are supported." });

        if (string.IsNullOrWhiteSpace(version))
            return BadRequest(new { message = "Version is required (e.g. 0.1.2)." });

        version = version.Trim().TrimStart('v');
        channel = (channel?.Trim().ToLowerInvariant()) switch
        {
            "beta" => "beta",
            _ => "stable"
        };

        // Ensure directory exists: wwwroot/desktop-updates/{channel}/
        var channelDir = Path.Combine(_env.WebRootPath, "desktop-updates", channel);
        Directory.CreateDirectory(channelDir);

        // Save package file
        var packageFileName = file.FileName.Trim();
        var packagePath = Path.Combine(channelDir, packageFileName);
        await using (var stream = System.IO.File.Create(packagePath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        // Build and write manifest.json
        var manifest = new DesktopUpdateManifest
        {
            Version = version,
            PackageUri = packageFileName,   // relative — resolves from manifest URL
            Notes = string.IsNullOrWhiteSpace(notes) ? $"Release {version}" : notes.Trim(),
            PublishedAt = DateTime.UtcNow
        };

        var manifestPath = Path.Combine(channelDir, "manifest.json");
        var manifestJson = JsonSerializer.Serialize(manifest, _json);
        await System.IO.File.WriteAllTextAsync(manifestPath, manifestJson, cancellationToken);

        _logger.LogInformation(
            "Desktop update published: channel={Channel} version={Version} package={Package}",
            channel, version, packageFileName);

        return Ok(manifest);
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
}
