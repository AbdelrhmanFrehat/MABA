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
    public bool IsValid { get; set; } = true;
    public string? ValidationMessage { get; set; }
    public decimal LengthMm => (decimal)Math.Sqrt((double)(((EndX - StartX) * (EndX - StartX)) + ((EndY - StartY) * (EndY - StartY)) + ((EndZ - StartZ) * (EndZ - StartZ))));
    public bool IsRapidMove => MotionType == GcodeMotionType.Rapid;
    public bool IsCutMove => MotionType == GcodeMotionType.Linear;
    public bool IsExecutable =>
        IsValid && (StartX != EndX || StartY != EndY || StartZ != EndZ);
}
