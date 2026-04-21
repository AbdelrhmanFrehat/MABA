namespace MabaControlCenter.Models;

public class CncDeviceStatusSnapshot
{
    public bool IsResponsive { get; set; }
    public bool IsReady { get; set; }
    public CncDeviceState DeviceState { get; set; } = CncDeviceState.Disconnected;
    public bool HasReportedPosition { get; set; }
    public decimal? ReportedX { get; set; }
    public decimal? ReportedY { get; set; }
    public decimal? ReportedZ { get; set; }
    public string? LastAcknowledgement { get; set; }
    public DateTime? LastAcknowledgedAt { get; set; }
    public string? LastProtocolError { get; set; }
    public string? LastStatusText { get; set; }

    public string PositionTrackingMode => HasReportedPosition
        ? "Device-reported position"
        : "Estimated position (app-tracked)";
}
