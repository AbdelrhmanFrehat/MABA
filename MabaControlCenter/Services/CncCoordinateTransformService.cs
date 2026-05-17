using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class CncCoordinateTransformService : ICncCoordinateTransformService
{
    public CncCoordinateSystemState CreateState(
        decimal machineX,
        decimal machineY,
        decimal machineZ,
        CncWorkOffset? workOffset = null,
        CncJobPlacementOffset? placementOffset = null,
        CncMachineReferenceState? referenceState = null,
        CncCoordinateMode coordinateMode = CncCoordinateMode.Work)
    {
        var work = workOffset?.Clone() ?? new CncWorkOffset();
        var placement = placementOffset?.Clone() ?? new CncJobPlacementOffset();
        var reference = referenceState?.Clone() ?? new CncMachineReferenceState();

        return new CncCoordinateSystemState
        {
            MachineX = machineX,
            MachineY = machineY,
            MachineZ = machineZ,
            WorkX = machineX - work.X,
            WorkY = machineY - work.Y,
            WorkZ = machineZ - work.Z,
            ActiveWorkOffset = work,
            JobPlacementOffset = placement,
            ReferenceState = reference,
            CoordinateMode = coordinateMode
        };
    }

    public CncCoordinateTransformResult WorkToMachine(decimal workX, decimal workY, decimal workZ, CncCoordinateSystemState state)
    {
        var placement = ApplyJobPlacement(workX, workY, workZ, state);
        placement.FinalMachineX = placement.WorkTargetX + placement.WorkOffsetX;
        placement.FinalMachineY = placement.WorkTargetY + placement.WorkOffsetY;
        placement.FinalMachineZ = placement.WorkTargetZ + placement.WorkOffsetZ;
        placement.CoordinateMode = CncCoordinateMode.FlattenedForFirmware;
        return placement;
    }

    public CncCoordinateTransformResult MachineToWork(decimal machineX, decimal machineY, decimal machineZ, CncCoordinateSystemState state)
    {
        return new CncCoordinateTransformResult
        {
            RawGcodeX = machineX,
            RawGcodeY = machineY,
            RawGcodeZ = machineZ,
            WorkTargetX = machineX - state.ActiveWorkOffset.X - state.JobPlacementOffset.X,
            WorkTargetY = machineY - state.ActiveWorkOffset.Y - state.JobPlacementOffset.Y,
            WorkTargetZ = machineZ - state.ActiveWorkOffset.Z - state.JobPlacementOffset.Z,
            WorkOffsetX = state.ActiveWorkOffset.X,
            WorkOffsetY = state.ActiveWorkOffset.Y,
            WorkOffsetZ = state.ActiveWorkOffset.Z,
            PlacementOffsetX = state.JobPlacementOffset.X,
            PlacementOffsetY = state.JobPlacementOffset.Y,
            PlacementOffsetZ = state.JobPlacementOffset.Z,
            FinalMachineX = machineX,
            FinalMachineY = machineY,
            FinalMachineZ = machineZ,
            CoordinateMode = CncCoordinateMode.Machine
        };
    }

    public CncCoordinateTransformResult FlattenForFirmware(decimal rawWorkX, decimal rawWorkY, decimal rawWorkZ, CncCoordinateSystemState state)
    {
        return WorkToMachine(rawWorkX, rawWorkY, rawWorkZ, state);
    }

    public CncCoordinateTransformResult ApplyJobPlacement(decimal workX, decimal workY, decimal workZ, CncCoordinateSystemState state)
    {
        return new CncCoordinateTransformResult
        {
            RawGcodeX = workX,
            RawGcodeY = workY,
            RawGcodeZ = workZ,
            WorkTargetX = workX + state.JobPlacementOffset.X,
            WorkTargetY = workY + state.JobPlacementOffset.Y,
            WorkTargetZ = workZ + state.JobPlacementOffset.Z,
            WorkOffsetX = state.ActiveWorkOffset.X,
            WorkOffsetY = state.ActiveWorkOffset.Y,
            WorkOffsetZ = state.ActiveWorkOffset.Z,
            PlacementOffsetX = state.JobPlacementOffset.X,
            PlacementOffsetY = state.JobPlacementOffset.Y,
            PlacementOffsetZ = state.JobPlacementOffset.Z,
            CoordinateMode = CncCoordinateMode.Preview
        };
    }

    public string? ValidateBounds(decimal machineX, decimal machineY, decimal machineZ, CncMachineBounds bounds, CncMachineConfig config)
    {
        if (config.SoftLimitsEnabled)
        {
            if (machineX < bounds.XMin || machineX > bounds.XMax)
                return $"Bounds violation: X {machineX:0.###} mm is outside [{bounds.XMin:0.###}, {bounds.XMax:0.###}] mm.";
            if (machineY < bounds.YMin || machineY > bounds.YMax)
                return $"Bounds violation: Y {machineY:0.###} mm is outside [{bounds.YMin:0.###}, {bounds.YMax:0.###}] mm.";
            if (ShouldEnforceAbsoluteZBounds(config) && (machineZ < bounds.ZMin || machineZ > bounds.ZMax))
                return $"Bounds violation: Z {machineZ:0.###} mm is outside [{bounds.ZMin:0.###}, {bounds.ZMax:0.###}] mm.";
        }

        if (!config.SupportsXAxis && machineX != bounds.XMin)
            return "The active machine profile does not support X axis motion.";
        if (!config.SupportsYAxis && machineY != bounds.YMin)
            return "The active machine profile does not support Y axis motion.";
        if (!config.SupportsZAxis && machineZ != bounds.ZMin)
            return "The active machine profile does not support Z axis motion.";

        return null;
    }

    private static bool ShouldEnforceAbsoluteZBounds(CncMachineConfig config)
    {
        // If the machine does not home Z, absolute machine Z is not trustworthy after power loss
        // or manual repositioning. In that setup we rely on manual Z zero + job preflight instead
        // of pretending a fixed [ZMin, ZMax] machine-space range is authoritative.
        return config.SupportsZAxis && config.HomeZEnabled;
    }

    public CncFrameBounds ComputeFrameBounds(IReadOnlyList<GcodeMotionCommand> motions)
    {
        var executable = motions.Where(m => m.IsExecutable).ToList();
        if (executable.Count == 0)
            return new CncFrameBounds();

        return new CncFrameBounds
        {
            MinX = executable.Min(m => Math.Min(m.StartX, m.EndX)),
            MaxX = executable.Max(m => Math.Max(m.StartX, m.EndX)),
            MinY = executable.Min(m => Math.Min(m.StartY, m.EndY)),
            MaxY = executable.Max(m => Math.Max(m.StartY, m.EndY))
        };
    }

    public string ExplainTransform(CncCoordinateTransformResult result)
    {
        return result.ExplainTransform();
    }

    public CncCoordinateTransformResult Validate(CncCoordinateTransformResult result, CncMachineBounds bounds, CncMachineConfig config)
    {
        result.BoundsMessage = ValidateBounds(result.FinalMachineX, result.FinalMachineY, result.FinalMachineZ, bounds, config);
        result.IsWithinBounds = string.IsNullOrWhiteSpace(result.BoundsMessage);
        return result;
    }
}
