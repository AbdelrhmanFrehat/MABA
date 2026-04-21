namespace MabaControlCenter.Models;

public class CncDriverCapabilities
{
    public bool SupportsZHoming { get; set; }
    public bool SupportsAcknowledgements { get; set; }
    public bool SupportsLivePositionReporting { get; set; }
    public bool SupportsCombinedXyMove { get; set; }
    public bool SupportsPause { get; set; }
    public bool SupportsAlarmReset { get; set; }
    public bool SupportsWorkCoordinateSystem { get; set; }
    public string VisualizationModelType { get; set; } = "Gantry3Axis";

    public string Summary =>
        $"Ack: {SupportsAcknowledgements}, Pos: {SupportsLivePositionReporting}, Z Home: {SupportsZHoming}, XY Combo: {SupportsCombinedXyMove}, Pause: {SupportsPause}, Reset: {SupportsAlarmReset}";
}
