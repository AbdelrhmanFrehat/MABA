using Maba.Domain.Common;

namespace Maba.Domain.ControlCenter;

/// <summary>
/// Tracks the compatibility stack for a desktop app release:
/// which firmware version, protocol, and board it was built against.
/// One record per release. Not a changelog — just the current known stack.
/// </summary>
public class AppRuntimeMetadata : BaseEntity
{
    public string Channel { get; set; } = "stable";

    // Desktop app side
    public string AppVersion { get; set; } = string.Empty;

    // Firmware / machine-side
    public string FirmwareName { get; set; } = string.Empty;
    public string FirmwareVersion { get; set; } = string.Empty;
    public string TargetBoard { get; set; } = string.Empty;
    public string ProtocolName { get; set; } = string.Empty;

    // Human-readable summaries (not source code)
    public string? CommandSummary { get; set; }
    public string? CompatibilityNotes { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime? PublishedAt { get; set; }
}
