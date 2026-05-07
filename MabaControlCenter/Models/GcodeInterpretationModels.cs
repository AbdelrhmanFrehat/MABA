using System.Collections.ObjectModel;

namespace MabaControlCenter.Models;

public enum GcodeUnitMode
{
    Millimeters,
    Inches
}

public enum GcodeDistanceMode
{
    Absolute,
    Incremental
}

public enum GcodePlane
{
    XY,
    XZ,
    YZ
}

public enum GcodeModalMotionMode
{
    None,
    Rapid,
    Linear,
    ArcClockwise,
    ArcCounterClockwise
}

public enum GcodeSpindleState
{
    Off,
    Clockwise
}

public enum GcodeCoordinateSpace
{
    Work,
    Machine,
    JobPreview
}

public class GcodeCoordinateState
{
    public decimal MachineX { get; set; }
    public decimal MachineY { get; set; }
    public decimal MachineZ { get; set; }
    public decimal WorkX { get; set; }
    public decimal WorkY { get; set; }
    public decimal WorkZ { get; set; }
    public decimal WorkOffsetX { get; set; }
    public decimal WorkOffsetY { get; set; }
    public decimal WorkOffsetZ { get; set; }
    public decimal PlacementOffsetX { get; set; }
    public decimal PlacementOffsetY { get; set; }
    public decimal PlacementOffsetZ { get; set; }

    public GcodeCoordinateState Clone()
    {
        return (GcodeCoordinateState)MemberwiseClone();
    }
}

public class GcodeModalState
{
    public GcodeUnitMode Units { get; set; } = GcodeUnitMode.Millimeters;
    public GcodeDistanceMode DistanceMode { get; set; } = GcodeDistanceMode.Absolute;
    public GcodeModalMotionMode MotionMode { get; set; } = GcodeModalMotionMode.Rapid;
    public GcodePlane Plane { get; set; } = GcodePlane.XY;
    public decimal FeedRateMmPerMinute { get; set; } = 0m;
    public GcodeSpindleState SpindleState { get; set; } = GcodeSpindleState.Off;
    public decimal? SpindleSpeed { get; set; }
    public int CurrentWorkCoordinateSystem { get; set; } = 54;
    public int? ToolNumber { get; set; }
    public GcodeCoordinateState Coordinates { get; set; } = new();

    public GcodeModalState Clone()
    {
        return new GcodeModalState
        {
            Units = Units,
            DistanceMode = DistanceMode,
            MotionMode = MotionMode,
            Plane = Plane,
            FeedRateMmPerMinute = FeedRateMmPerMinute,
            SpindleState = SpindleState,
            SpindleSpeed = SpindleSpeed,
            CurrentWorkCoordinateSystem = CurrentWorkCoordinateSystem,
            ToolNumber = ToolNumber,
            Coordinates = Coordinates.Clone()
        };
    }
}

public class GcodeInterpretedCommand
{
    public int SourceLineNumber { get; set; }
    public string RawLine { get; set; } = string.Empty;
    public string SanitizedLine { get; set; } = string.Empty;
    public string? CommentText { get; set; }
    public bool IsIgnored { get; set; }
    public bool IsUnsupported { get; set; }
    public bool BlocksExecution { get; set; }
    public string? DiagnosticMessage { get; set; }
    public GcodeUnitMode Units { get; set; }
    public GcodeDistanceMode DistanceMode { get; set; }
    public GcodePlane Plane { get; set; }
    public GcodeModalMotionMode MotionMode { get; set; }
    public bool HasMotion { get; set; }
    public bool EmitsControllerCommand { get; set; }
    public bool IsSpindleChange { get; set; }
    public GcodeSpindleState SpindleState { get; set; }
    public decimal? SpindleSpeed { get; set; }
    public int? ToolNumber { get; set; }
    public decimal StartX { get; set; }
    public decimal StartY { get; set; }
    public decimal StartZ { get; set; }
    public decimal EndX { get; set; }
    public decimal EndY { get; set; }
    public decimal EndZ { get; set; }
    public decimal? FeedRateMmPerMinute { get; set; }
    public decimal? ArcOffsetI { get; set; }
    public decimal? ArcOffsetJ { get; set; }
    public decimal? ArcOffsetK { get; set; }
    public decimal? ArcCenterX { get; set; }
    public decimal? ArcCenterY { get; set; }
    public decimal? ArcCenterZ { get; set; }
    public decimal? ArcRadiusMm { get; set; }
    public decimal? ArcLengthMm { get; set; }
    public GcodeCoordinateSpace CoordinateSpace { get; set; } = GcodeCoordinateSpace.Work;
    public GcodeModalState ModalStateAfterLine { get; set; } = new();

    public GcodeMotionType? MotionType => MotionMode switch
    {
        GcodeModalMotionMode.Rapid => Models.GcodeMotionType.Rapid,
        GcodeModalMotionMode.Linear => Models.GcodeMotionType.Linear,
        GcodeModalMotionMode.ArcClockwise => Models.GcodeMotionType.ArcClockwise,
        GcodeModalMotionMode.ArcCounterClockwise => Models.GcodeMotionType.ArcCounterClockwise,
        _ => null
    };
}

public class GcodeInterpreterDiagnostics
{
    public ObservableCollection<string> Messages { get; } = new();
    public int WarningCount { get; set; }
    public int ErrorCount { get; set; }
    public int UnsupportedCommandCount { get; set; }
    public int UnsupportedPlaneCount { get; set; }

    public void AddMessage(string severity, string message)
    {
        Messages.Add($"[{severity}] {message}");
        if (severity.Equals("Error", StringComparison.OrdinalIgnoreCase))
            ErrorCount++;
        else
            WarningCount++;
    }
}

public class GcodeInterpreterResult
{
    public ObservableCollection<GcodeInterpretedCommand> Commands { get; } = new();
    public GcodeModalState FinalModalState { get; set; } = new();
    public GcodeInterpreterDiagnostics Diagnostics { get; set; } = new();
}
