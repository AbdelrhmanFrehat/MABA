using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface ICncJobPlacementService
{
    IReadOnlyList<GcodeMotionCommand> ApplyPlacement(IReadOnlyList<GcodeMotionCommand> motions, CncJobPlacement placement);
    CncFrameBounds CalculateBounds(IReadOnlyList<GcodeMotionCommand> motions);
    string? ValidatePlacement(IReadOnlyList<GcodeMotionCommand> originalMotions, CncJobPlacement placement, decimal machineMinX, decimal machineMaxX, decimal machineMinY, decimal machineMaxY);
    CncJobPlacement CreatePresetPlacement(IReadOnlyList<GcodeMotionCommand> originalMotions, CncPlacementPreset preset, decimal machineMinX, decimal machineMaxX, decimal machineMinY, decimal machineMaxY, decimal marginMm = 0m);
}
