using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class CncJobPlacementService : ICncJobPlacementService
{
    public IReadOnlyList<GcodeMotionCommand> ApplyPlacement(IReadOnlyList<GcodeMotionCommand> motions, CncJobPlacement placement)
    {
        return motions
            .Select(m => new GcodeMotionCommand
            {
                LineNumber = m.LineNumber,
                RawText = m.RawText,
                MotionType = m.MotionType,
                StartX = m.StartX + placement.OffsetX,
                StartY = m.StartY + placement.OffsetY,
                StartZ = m.StartZ,
                EndX = m.EndX + placement.OffsetX,
                EndY = m.EndY + placement.OffsetY,
                EndZ = m.EndZ,
                IsAbsoluteMode = m.IsAbsoluteMode,
                FeedRate = m.FeedRate,
                IsValid = m.IsValid,
                ValidationMessage = m.ValidationMessage
            })
            .ToList();
    }

    public CncFrameBounds CalculateBounds(IReadOnlyList<GcodeMotionCommand> motions)
    {
        var executable = motions.Where(m => m.IsExecutable).ToList();
        if (executable.Count == 0)
            return new CncFrameBounds();

        var points = executable
            .SelectMany(m => new[]
            {
                new { X = m.StartX, Y = m.StartY },
                new { X = m.EndX, Y = m.EndY }
            })
            .ToList();

        return new CncFrameBounds
        {
            MinX = points.Min(p => p.X),
            MaxX = points.Max(p => p.X),
            MinY = points.Min(p => p.Y),
            MaxY = points.Max(p => p.Y)
        };
    }

    public string? ValidatePlacement(IReadOnlyList<GcodeMotionCommand> originalMotions, CncJobPlacement placement, decimal machineMinX, decimal machineMaxX, decimal machineMinY, decimal machineMaxY)
    {
        var placedBounds = CalculateBounds(ApplyPlacement(originalMotions, placement));
        if (!placedBounds.IsValid)
            return "Load a valid toolpath before applying job placement.";

        if (placedBounds.MinX < machineMinX || placedBounds.MaxX > machineMaxX)
            return $"Placed job exceeds X bounds ({machineMinX:0.###} to {machineMaxX:0.###} mm).";

        if (placedBounds.MinY < machineMinY || placedBounds.MaxY > machineMaxY)
            return $"Placed job exceeds Y bounds ({machineMinY:0.###} to {machineMaxY:0.###} mm).";

        return null;
    }

    public CncJobPlacement CreatePresetPlacement(IReadOnlyList<GcodeMotionCommand> originalMotions, CncPlacementPreset preset, decimal machineMinX, decimal machineMaxX, decimal machineMinY, decimal machineMaxY, decimal marginMm = 0m)
    {
        var bounds = CalculateBounds(originalMotions);
        if (!bounds.IsValid)
            return new CncJobPlacement();

        var offsetX = preset switch
        {
            CncPlacementPreset.TopLeft or CncPlacementPreset.BottomLeft => (machineMinX + marginMm) - bounds.MinX,
            CncPlacementPreset.TopRight or CncPlacementPreset.BottomRight => (machineMaxX - marginMm - bounds.MaxX) + 0m,
            CncPlacementPreset.Center => ((machineMinX + machineMaxX) / 2m) - ((bounds.MinX + bounds.MaxX) / 2m),
            _ => 0m
        };

        var offsetY = preset switch
        {
            CncPlacementPreset.TopLeft or CncPlacementPreset.TopRight => (machineMinY + marginMm) - bounds.MinY,
            CncPlacementPreset.BottomLeft or CncPlacementPreset.BottomRight => (machineMaxY - marginMm - bounds.MaxY) + 0m,
            CncPlacementPreset.Center => ((machineMinY + machineMaxY) / 2m) - ((bounds.MinY + bounds.MaxY) / 2m),
            _ => 0m
        };

        return new CncJobPlacement
        {
            OffsetX = offsetX,
            OffsetY = offsetY
        };
    }
}
