namespace MabaControlCenter.Models;

public class CncPreviewSummary
{
    public decimal TotalDistanceMm { get; set; }
    public decimal RapidDistanceMm { get; set; }
    public decimal CutDistanceMm { get; set; }
    public TimeSpan EstimatedTime { get; set; }
}
