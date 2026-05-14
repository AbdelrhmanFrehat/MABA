namespace MabaControlCenter.Models;

public class CncDeviceStatusSnapshot
{
    public bool IsResponsive { get; set; }
    public bool IsReady { get; set; }
    public bool IsLocked { get; set; }
    public bool IsAlarmed { get; set; }
    public CncDeviceState DeviceState { get; set; } = CncDeviceState.Disconnected;
    public bool HasReportedPosition { get; set; }
    public decimal? ReportedX { get; set; }
    public decimal? ReportedY { get; set; }
    public decimal? ReportedZ { get; set; }
    public bool LimitXTriggered { get; set; }
    public bool LimitYTriggered { get; set; }
    public bool LimitZTriggered { get; set; }
    public string? LastAcknowledgement { get; set; }
    public DateTime? LastAcknowledgedAt { get; set; }
    public DateTime? LastVerifiedStatusAt { get; set; }
    public string? LastProtocolError { get; set; }
    public string? LastStatusText { get; set; }
    public string? LastMotionCommandText { get; set; }
    public decimal? LastMotionTargetX { get; set; }
    public decimal? LastMotionTargetY { get; set; }
    public decimal? LastMotionTargetZ { get; set; }
    public decimal? LastMotionDistanceMm { get; set; }
    public decimal? LastMotionFeedRateMmPerMinute { get; set; }
    public double? LastMotionEstimatedDurationMs { get; set; }
    public int? LastMotionTimeoutMs { get; set; }
    public string? FirmwareVersion { get; set; }
    public string? ProtocolVersion { get; set; }
    public CncControllerStatusConfidence StatusConfidence { get; set; } = CncControllerStatusConfidence.Unknown;
    public CncFirmwareIdentity FirmwareIdentity { get; set; } = new();
    public CncFirmwareCompatibilityResult FirmwareCompatibility { get; set; } = new();

    public string PositionTrackingMode => HasReportedPosition
        ? "Device-reported position"
        : "Estimated position (app-tracked)";
}
