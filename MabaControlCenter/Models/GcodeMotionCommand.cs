namespace MabaControlCenter.Models;

public class GcodeMotionCommand
{
    public int LineNumber { get; set; }
    public string RawText { get; set; } = string.Empty;
    public GcodeMotionType MotionType { get; set; }
    public decimal StartX { get; set; }
    public decimal StartY { get; set; }
    public decimal StartZ { get; set; }
    public decimal EndX { get; set; }
    public decimal EndY { get; set; }
    public decimal EndZ { get; set; }
    public bool IsAbsoluteMode { get; set; }
    public decimal? FeedRate { get; set; }
    public GcodeUnitMode Units { get; set; } = GcodeUnitMode.Millimeters;
    public GcodeDistanceMode DistanceMode { get; set; } = GcodeDistanceMode.Absolute;
    public GcodePlane Plane { get; set; } = GcodePlane.XY;
    public decimal? ArcOffsetI { get; set; }
    public decimal? ArcOffsetJ { get; set; }
    public decimal? ArcOffsetK { get; set; }
    public decimal? ArcCenterX { get; set; }
    public decimal? ArcCenterY { get; set; }
    public decimal? ArcCenterZ { get; set; }
    public decimal? ArcRadiusMm { get; set; }
    public decimal? ArcLengthMm { get; set; }
    public string? ModalSummary { get; set; }
    public bool IsValid { get; set; } = true;
    public string? ValidationMessage { get; set; }
    public decimal LengthMm => ArcLengthMm ?? (decimal)Math.Sqrt((double)(((EndX - StartX) * (EndX - StartX)) + ((EndY - StartY) * (EndY - StartY)) + ((EndZ - StartZ) * (EndZ - StartZ))));
    public bool IsRapidMove => MotionType == GcodeMotionType.Rapid;
    public bool IsCutMove => MotionType is GcodeMotionType.Linear or GcodeMotionType.ArcClockwise or GcodeMotionType.ArcCounterClockwise;
    public bool IsArcMove => MotionType is GcodeMotionType.ArcClockwise or GcodeMotionType.ArcCounterClockwise;
    public bool IsExecutable =>
        IsValid && (StartX != EndX || StartY != EndY || StartZ != EndZ);
}
