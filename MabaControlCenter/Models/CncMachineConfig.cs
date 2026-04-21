namespace MabaControlCenter.Models;

public class CncMachineConfig
{
    public int BaudRate { get; set; } = 115200;
    public decimal XStepsPerMm { get; set; } = 80m;
    public decimal YStepsPerMm { get; set; } = 80m;
    public decimal ZStepsPerMm { get; set; } = 400m;
    public decimal XMinMm { get; set; } = 0m;
    public decimal XLimitMm { get; set; } = 300m;
    public decimal YMinMm { get; set; } = 0m;
    public decimal YLimitMm { get; set; } = 300m;
    public decimal ZMinMm { get; set; } = 0m;
    public decimal ZLimitMm { get; set; } = 100m;
    public CncHomeOriginConvention HomeOriginConvention { get; set; } = CncHomeOriginConvention.TopLeft;
    public bool HomeXEnabled { get; set; } = true;
    public bool HomeYEnabled { get; set; } = true;
    public bool HomeZEnabled { get; set; }
    public bool SupportsXAxis { get; set; } = true;
    public bool SupportsYAxis { get; set; } = true;
    public bool SupportsZAxis { get; set; } = true;
    public bool SoftLimitsEnabled { get; set; } = true;
    public CncDriverType DriverType { get; set; } = CncDriverType.ArduinoSerial;
    public List<decimal> JogPresets { get; set; } = new() { 0.1m, 1m, 10m };
    public decimal VisualizationWidthMm { get; set; } = 300m;
    public decimal VisualizationHeightMm { get; set; } = 300m;
    public decimal VisualizationDepthMm { get; set; } = 100m;

    public CncMachineBounds Bounds => new()
    {
        XMin = XMinMm,
        XMax = XLimitMm,
        YMin = YMinMm,
        YMax = YLimitMm,
        ZMin = ZMinMm,
        ZMax = ZLimitMm
    };
}
