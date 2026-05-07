using System.Globalization;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class CncExecutionPlannerService : ICncExecutionPlanner
{
    private readonly IActiveMachineContextService _activeMachineContextService;
    private readonly List<GcodeMotionCommand> _plannedMotions = new();
    private readonly List<GcodeInterpretedCommand> _interpretedCommands = new();

    public CncExecutionPlannerService(IActiveMachineContextService activeMachineContextService)
    {
        _activeMachineContextService = activeMachineContextService;
    }

    public bool HasLoadedPlan => PlannedMotions.Count > 0;
    public IReadOnlyList<GcodeMotionCommand> PlannedMotions => _plannedMotions;
    public IReadOnlyList<GcodeInterpretedCommand> InterpretedCommands => _interpretedCommands;

    public CncStreamingSession CreatePlan(IReadOnlyList<GcodeInterpretedCommand> commands, ICncControllerService controllerService, string? activeJobName)
    {
        _interpretedCommands.Clear();
        _interpretedCommands.AddRange(commands);
        _plannedMotions.Clear();
        _plannedMotions.AddRange(commands.Where(c => c.HasMotion && c.MotionType != null).Select(ToMotion));

        var session = new CncStreamingSession
        {
            State = CncStreamingState.Planning,
            ActiveJobName = string.IsNullOrWhiteSpace(activeJobName) ? "Loaded CNC Job" : activeJobName
        };

        var driverType = _activeMachineContextService.Current.DriverType;
        var firmwareProtocol = _activeMachineContextService.Current.MachineDefinition?.RuntimeBinding.FirmwareProtocol;
        var sequence = 0;

        foreach (var command in _interpretedCommands)
        {
            foreach (var planned in CreatePlannedCommands(command, controllerService, driverType, firmwareProtocol))
            {
                planned.SequenceIndex = sequence++;
                session.PlannedCommands.Add(planned);
            }
        }

        session.State = CncStreamingState.Preflight;
        session.LastMessage = $"Planned {session.PlannedCommands.Count} controller command(s) from {_interpretedCommands.Count} interpreted line(s).";
        session.Diagnostics.AddEvent(session.LastMessage);
        return session;
    }

    private IEnumerable<CncPlannedCommand> CreatePlannedCommands(
        GcodeInterpretedCommand command,
        ICncControllerService controllerService,
        DriverType driverType,
        FirmwareProtocol? firmwareProtocol)
    {
        if (command.IsUnsupported || command.BlocksExecution)
            yield break;

        if (command.IsSpindleChange)
        {
            yield return new CncPlannedCommand
            {
                CommandText = command.SpindleState == GcodeSpindleState.Clockwise ? "M3" : "M5",
                SourceLineNumber = command.SourceLineNumber,
                MotionType = GcodeMotionType.Linear,
                RequiresAck = true,
                SafetyCategory = CncCommandSafetyCategory.System,
                Metadata = command.RawLine,
                CoordinateSpace = command.CoordinateSpace,
                SpindleState = command.SpindleState,
                OriginalRawLine = command.RawLine
            };
            yield break;
        }

        if (!command.HasMotion || command.MotionType == null)
            yield break;

        var effectiveFeed = command.FeedRateMmPerMinute ?? 300m;
        if (driverType == DriverType.Simulated || firmwareProtocol == FirmwareProtocol.MabaProtocol)
        {
            if (command.MotionType is GcodeMotionType.ArcClockwise or GcodeMotionType.ArcCounterClockwise)
            {
                yield return new CncPlannedCommand
                {
                    CommandText = string.Create(
                        CultureInfo.InvariantCulture,
                        $"{(command.MotionType == GcodeMotionType.ArcClockwise ? "G2" : "G3")} X{command.EndX:0.###} Y{command.EndY:0.###} I{(command.ArcOffsetI ?? 0m):0.###} J{(command.ArcOffsetJ ?? 0m):0.###} F{effectiveFeed:0.###}"),
                    SourceLineNumber = command.SourceLineNumber,
                    MotionType = command.MotionType.Value,
                    ExpectedEndX = command.EndX,
                    ExpectedEndY = command.EndY,
                    ExpectedEndZ = command.EndZ,
                    EstimatedDistanceMm = command.ArcLengthMm ?? ToMotion(command).LengthMm,
                    RequiresAck = true,
                    SafetyCategory = CncCommandSafetyCategory.Motion,
                    Metadata = command.RawLine,
                    CoordinateSpace = command.CoordinateSpace,
                    FeedRateMmPerMinute = effectiveFeed,
                    OriginalRawLine = command.RawLine
                };
                yield break;
            }

            yield return new CncPlannedCommand
            {
                CommandText = string.Create(
                    CultureInfo.InvariantCulture,
                    $"{(command.MotionType == GcodeMotionType.Rapid ? "G0" : "G1")} X{command.EndX:0.###} Y{command.EndY:0.###} Z{command.EndZ:0.###} F{effectiveFeed:0.###}"),
                SourceLineNumber = command.SourceLineNumber,
                MotionType = command.MotionType.Value,
                ExpectedEndX = command.EndX,
                ExpectedEndY = command.EndY,
                ExpectedEndZ = command.EndZ,
                EstimatedDistanceMm = ToMotion(command).LengthMm,
                RequiresAck = true,
                SafetyCategory = CncCommandSafetyCategory.Motion,
                Metadata = command.RawLine,
                CoordinateSpace = command.CoordinateSpace,
                FeedRateMmPerMinute = effectiveFeed,
                OriginalRawLine = command.RawLine
            };
            yield break;
        }

        foreach (var legacyCommand in CreateLegacyCommands(command, controllerService.Config))
            yield return legacyCommand;
    }

    private IEnumerable<CncPlannedCommand> CreateLegacyCommands(GcodeInterpretedCommand command, CncMachineConfig profile)
    {
        if (command.MotionType is GcodeMotionType.ArcClockwise or GcodeMotionType.ArcCounterClockwise)
        {
            var sampled = GcodeMotionGeometry.SamplePath(ToMotion(command), minimumSegments: 60);
            for (var i = 0; i < sampled.Count - 1; i++)
            {
                var start = sampled[i];
                var end = sampled[i + 1];
                foreach (var segmented in CreateLegacyLinearCommands(command, profile, start.X, start.Y, start.Z, end.X, end.Y, end.Z))
                    yield return segmented;
            }

            yield break;
        }

        foreach (var linear in CreateLegacyLinearCommands(command, profile, command.StartX, command.StartY, command.StartZ, command.EndX, command.EndY, command.EndZ))
            yield return linear;
    }

    private IEnumerable<CncPlannedCommand> CreateLegacyLinearCommands(GcodeInterpretedCommand command, CncMachineConfig profile, decimal startX, decimal startY, decimal startZ, decimal endX, decimal endY, decimal endZ)
    {
        var deltaX = endX - startX;
        var deltaY = endY - startY;
        var deltaZ = endZ - startZ;
        var xSteps = ToSteps(deltaX, profile.XStepsPerMm);
        var ySteps = ToSteps(deltaY, profile.YStepsPerMm);
        var zSteps = ToSteps(deltaZ, profile.ZStepsPerMm);

        if (xSteps > 0 && ySteps > 0)
        {
            yield return new CncPlannedCommand
            {
                CommandText = $"XY,{(deltaX >= 0m ? xSteps : -xSteps)},{(deltaY >= 0m ? ySteps : -ySteps)}",
                SourceLineNumber = command.SourceLineNumber,
                MotionType = GcodeMotionType.Linear,
                ExpectedEndX = endX,
                ExpectedEndY = endY,
                ExpectedEndZ = startZ,
                EstimatedDistanceMm = (decimal)Math.Sqrt((double)((deltaX * deltaX) + (deltaY * deltaY))),
                RequiresAck = false,
                SafetyCategory = CncCommandSafetyCategory.Motion,
                Metadata = command.RawLine,
                CoordinateSpace = command.CoordinateSpace,
                FeedRateMmPerMinute = command.FeedRateMmPerMinute,
                OriginalRawLine = command.RawLine
            };
        }
        else
        {
            if (xSteps > 0)
                yield return BuildLegacyAxisCommand("x", command, endX, startY, startZ, xSteps, deltaX >= 0m, Math.Abs(deltaX));
            if (ySteps > 0)
                yield return BuildLegacyAxisCommand("y", command, startX, endY, startZ, ySteps, deltaY >= 0m, Math.Abs(deltaY));
        }

        if (zSteps > 0)
            yield return BuildLegacyAxisCommand("z", command, endX, endY, endZ, zSteps, deltaZ >= 0m, Math.Abs(deltaZ));
    }

    private static CncPlannedCommand BuildLegacyAxisCommand(string axis, GcodeInterpretedCommand command, decimal endX, decimal endY, decimal endZ, int steps, bool positive, decimal distance)
    {
        return new CncPlannedCommand
        {
            CommandText = $"{(positive ? "+" : "-")}{steps}{axis}",
            SourceLineNumber = command.SourceLineNumber,
            MotionType = GcodeMotionType.Linear,
            ExpectedEndX = endX,
            ExpectedEndY = endY,
            ExpectedEndZ = endZ,
            EstimatedDistanceMm = distance,
            RequiresAck = false,
            SafetyCategory = CncCommandSafetyCategory.Motion,
            Metadata = command.RawLine,
            CoordinateSpace = command.CoordinateSpace,
            FeedRateMmPerMinute = command.FeedRateMmPerMinute,
            OriginalRawLine = command.RawLine
        };
    }

    private static int ToSteps(decimal deltaMm, decimal stepsPerMm)
    {
        return (int)Math.Round(Math.Abs(deltaMm * stepsPerMm), MidpointRounding.AwayFromZero);
    }

    private static GcodeMotionCommand ToMotion(GcodeInterpretedCommand command)
    {
        return new GcodeMotionCommand
        {
            LineNumber = command.SourceLineNumber,
            RawText = command.RawLine,
            MotionType = command.MotionType ?? GcodeMotionType.Linear,
            StartX = command.StartX,
            StartY = command.StartY,
            StartZ = command.StartZ,
            EndX = command.EndX,
            EndY = command.EndY,
            EndZ = command.EndZ,
            IsAbsoluteMode = command.DistanceMode == GcodeDistanceMode.Absolute,
            FeedRate = command.FeedRateMmPerMinute,
            Units = command.Units,
            DistanceMode = command.DistanceMode,
            Plane = command.Plane,
            ArcOffsetI = command.ArcOffsetI,
            ArcOffsetJ = command.ArcOffsetJ,
            ArcOffsetK = command.ArcOffsetK,
            ArcCenterX = command.ArcCenterX,
            ArcCenterY = command.ArcCenterY,
            ArcCenterZ = command.ArcCenterZ,
            ArcRadiusMm = command.ArcRadiusMm,
            ArcLengthMm = command.ArcLengthMm,
            IsValid = !command.BlocksExecution,
            ValidationMessage = command.DiagnosticMessage
        };
    }
}
