namespace MabaControlCenter.Models;

[Flags]
public enum CncHomedAxes
{
    None = 0,
    X = 1,
    Y = 2,
    Z = 4
}

public enum CncReferenceLostReason
{
    None,
    UnknownOnConnect,
    Disconnect,
    Alarm,
    Reset,
    UnlockWithoutReference,
    FaultRecoveryRequired
}

public enum CncCoordinateMode
{
    Machine,
    Work,
    Preview,
    FlattenedForFirmware
}

public class CncWorkOffset
{
    public decimal X { get; set; }
    public decimal Y { get; set; }
    public decimal Z { get; set; }

    public CncWorkOffset Clone()
    {
        return new CncWorkOffset
        {
            X = X,
            Y = Y,
            Z = Z
        };
    }
}

public class CncJobPlacementOffset
{
    public decimal X { get; set; }
    public decimal Y { get; set; }
    public decimal Z { get; set; }

    public CncJobPlacementOffset Clone()
    {
        return new CncJobPlacementOffset
        {
            X = X,
            Y = Y,
            Z = Z
        };
    }
}

public class CncMachineReferenceState
{
    public bool IsHomed { get; set; }
    public CncHomedAxes HomedAxes { get; set; } = CncHomedAxes.None;
    public bool ReferenceValid { get; set; }
    public CncReferenceLostReason ReferenceLostReason { get; set; } = CncReferenceLostReason.None;
    public DateTime? LastHomedAt { get; set; }
    public DateTime? LastZeroedAt { get; set; }

    public string StatusText => ReferenceLostReason switch
    {
        CncReferenceLostReason.None when IsHomed && ReferenceValid => "Homed",
        CncReferenceLostReason.None when !IsHomed => "Not Homed",
        CncReferenceLostReason.UnknownOnConnect => "Reference Unknown",
        CncReferenceLostReason.Disconnect => "Reference Unknown",
        CncReferenceLostReason.Alarm => "Reference Lost",
        CncReferenceLostReason.Reset => "Reference Unknown",
        CncReferenceLostReason.UnlockWithoutReference => "Reference Unknown",
        CncReferenceLostReason.FaultRecoveryRequired => "Reference Lost",
        _ => "Reference Unknown"
    };

    public string? WarningText => ReferenceLostReason switch
    {
        CncReferenceLostReason.None when ReferenceValid => null,
        CncReferenceLostReason.None => "Machine reference has not been established yet.",
        CncReferenceLostReason.UnknownOnConnect => "Machine connected, but the reference origin is unknown until homing is completed.",
        CncReferenceLostReason.Disconnect => "Reference became unknown after disconnect. Reconnect and revalidate the machine before running.",
        CncReferenceLostReason.Alarm => "Reference was lost after alarm/fault. Re-home before running.",
        CncReferenceLostReason.Reset => "Controller was reset. Revalidate the machine reference before running.",
        CncReferenceLostReason.UnlockWithoutReference => "Machine was unlocked without a trusted reference. Home before running.",
        CncReferenceLostReason.FaultRecoveryRequired => "Recovery is required before the machine reference can be trusted again.",
        _ => "Machine reference is not valid."
    };

    public CncMachineReferenceState Clone()
    {
        return new CncMachineReferenceState
        {
            IsHomed = IsHomed,
            HomedAxes = HomedAxes,
            ReferenceValid = ReferenceValid,
            ReferenceLostReason = ReferenceLostReason,
            LastHomedAt = LastHomedAt,
            LastZeroedAt = LastZeroedAt
        };
    }
}

public class CncCoordinateSystemState
{
    public decimal MachineX { get; set; }
    public decimal MachineY { get; set; }
    public decimal MachineZ { get; set; }
    public decimal WorkX { get; set; }
    public decimal WorkY { get; set; }
    public decimal WorkZ { get; set; }
    public CncWorkOffset ActiveWorkOffset { get; set; } = new();
    public CncJobPlacementOffset JobPlacementOffset { get; set; } = new();
    public CncMachineReferenceState ReferenceState { get; set; } = new();
    public CncCoordinateMode CoordinateMode { get; set; } = CncCoordinateMode.Work;

    public CncCoordinateSystemState Clone()
    {
        return new CncCoordinateSystemState
        {
            MachineX = MachineX,
            MachineY = MachineY,
            MachineZ = MachineZ,
            WorkX = WorkX,
            WorkY = WorkY,
            WorkZ = WorkZ,
            ActiveWorkOffset = ActiveWorkOffset.Clone(),
            JobPlacementOffset = JobPlacementOffset.Clone(),
            ReferenceState = ReferenceState.Clone(),
            CoordinateMode = CoordinateMode
        };
    }
}

public class CncCoordinateTransformResult
{
    public decimal RawGcodeX { get; set; }
    public decimal RawGcodeY { get; set; }
    public decimal RawGcodeZ { get; set; }
    public decimal WorkTargetX { get; set; }
    public decimal WorkTargetY { get; set; }
    public decimal WorkTargetZ { get; set; }
    public decimal WorkOffsetX { get; set; }
    public decimal WorkOffsetY { get; set; }
    public decimal WorkOffsetZ { get; set; }
    public decimal PlacementOffsetX { get; set; }
    public decimal PlacementOffsetY { get; set; }
    public decimal PlacementOffsetZ { get; set; }
    public decimal FinalMachineX { get; set; }
    public decimal FinalMachineY { get; set; }
    public decimal FinalMachineZ { get; set; }
    public bool IsWithinBounds { get; set; }
    public string? BoundsMessage { get; set; }
    public CncCoordinateMode CoordinateMode { get; set; } = CncCoordinateMode.FlattenedForFirmware;

    public string ExplainTransform()
    {
        return $"Raw {RawGcodeX:0.###},{RawGcodeY:0.###},{RawGcodeZ:0.###} -> Work {WorkTargetX:0.###},{WorkTargetY:0.###},{WorkTargetZ:0.###} + WorkOffset {WorkOffsetX:0.###},{WorkOffsetY:0.###},{WorkOffsetZ:0.###} + Placement {PlacementOffsetX:0.###},{PlacementOffsetY:0.###},{PlacementOffsetZ:0.###} = Machine {FinalMachineX:0.###},{FinalMachineY:0.###},{FinalMachineZ:0.###}";
    }
}
