using System.Globalization;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class CncExecutionPlannerService : ICncExecutionPlanner
{
    private readonly IActiveMachineContextService _activeMachineContextService;

    public CncExecutionPlannerService(IActiveMachineContextService activeMachineContextService)
    {
        _activeMachineContextService = activeMachineContextService;
    }

    public bool HasLoadedPlan => PlannedMotions.Count > 0;
    public IReadOnlyList<GcodeMotionCommand> PlannedMotions => _plannedMotions;

    private readonly List<GcodeMotionCommand> _plannedMotions = new();

    public CncStreamingSession CreatePlan(IReadOnlyList<GcodeMotionCommand> motions, ICncControllerService controllerService, string? activeJobName)
    {
        _plannedMotions.Clear();
        _plannedMotions.AddRange(motions.Where(m => m.IsExecutable));

        var session = new CncStreamingSession
        {
            State = CncStreamingState.Planning,
            ActiveJobName = string.IsNullOrWhiteSpace(activeJobName) ? "Loaded CNC Job" : activeJobName
        };

        var driverType = _activeMachineContextService.Current.DriverType;
        var profile = controllerService.Config;
        var sequence = 0;

        foreach (var motion in _plannedMotions)
        {
            foreach (var command in CreateCommandsForMotion(motion, controllerService, profile, driverType))
            {
                command.SequenceIndex = sequence++;
                session.PlannedCommands.Add(command);
            }
        }

        session.State = CncStreamingState.Preflight;
        session.LastMessage = $"Planned {session.PlannedCommands.Count} controller command(s).";
        session.Diagnostics.AddEvent(session.LastMessage);
        return session;
    }

    private IEnumerable<CncPlannedCommand> CreateCommandsForMotion(
        GcodeMotionCommand motion,
        ICncControllerService controllerService,
        CncMachineConfig profile,
        DriverType driverType)
    {
        if (driverType == DriverType.Simulated || _activeMachineContextService.Current.MachineDefinition?.RuntimeBinding.FirmwareProtocol == FirmwareProtocol.MabaProtocol)
        {
            yield return new CncPlannedCommand
            {
                CommandText = string.Create(
                    CultureInfo.InvariantCulture,
                    $"{(motion.IsRapidMove ? "G0" : "G1")} X{motion.EndX:0.###} Y{motion.EndY:0.###} Z{motion.EndZ:0.###} F{(motion.FeedRate ?? 300m):0.###}"),
                SourceLineNumber = motion.LineNumber,
                MotionType = motion.MotionType,
                ExpectedEndX = motion.EndX,
                ExpectedEndY = motion.EndY,
                ExpectedEndZ = motion.EndZ,
                EstimatedDistanceMm = motion.LengthMm,
                RequiresAck = true,
                SafetyCategory = CncCommandSafetyCategory.Motion,
                Metadata = motion.RawText
            };
            yield break;
        }

        var deltaX = motion.EndX - motion.StartX;
        var deltaY = motion.EndY - motion.StartY;
        var deltaZ = motion.EndZ - motion.StartZ;
        var xSteps = ToSteps(deltaX, profile.XStepsPerMm);
        var ySteps = ToSteps(deltaY, profile.YStepsPerMm);
        var zSteps = ToSteps(deltaZ, profile.ZStepsPerMm);

        if (xSteps > 0 && ySteps > 0)
        {
            yield return new CncPlannedCommand
            {
                CommandText = $"XY,{(deltaX >= 0m ? xSteps : -xSteps)},{(deltaY >= 0m ? ySteps : -ySteps)}",
                SourceLineNumber = motion.LineNumber,
                MotionType = motion.MotionType,
                ExpectedEndX = motion.EndX,
                ExpectedEndY = motion.EndY,
                ExpectedEndZ = motion.StartZ,
                EstimatedDistanceMm = (decimal)Math.Sqrt((double)((deltaX * deltaX) + (deltaY * deltaY))),
                RequiresAck = false,
                SafetyCategory = CncCommandSafetyCategory.Motion,
                Metadata = motion.RawText
            };
        }
        else
        {
            if (xSteps > 0)
                yield return BuildLegacyAxisCommand("x", motion, motion.EndX, motion.StartY, motion.StartZ, xSteps, deltaX >= 0m, Math.Abs(deltaX));
            if (ySteps > 0)
                yield return BuildLegacyAxisCommand("y", motion, motion.StartX, motion.EndY, motion.StartZ, ySteps, deltaY >= 0m, Math.Abs(deltaY));
        }

        if (zSteps > 0)
            yield return BuildLegacyAxisCommand("z", motion, motion.EndX, motion.EndY, motion.EndZ, zSteps, deltaZ >= 0m, Math.Abs(deltaZ));
    }

    private static CncPlannedCommand BuildLegacyAxisCommand(string axis, GcodeMotionCommand motion, decimal endX, decimal endY, decimal endZ, int steps, bool positive, decimal distance)
    {
        return new CncPlannedCommand
        {
            CommandText = $"{(positive ? "+" : "-")}{steps}{axis}",
            SourceLineNumber = motion.LineNumber,
            MotionType = motion.MotionType,
            ExpectedEndX = endX,
            ExpectedEndY = endY,
            ExpectedEndZ = endZ,
            EstimatedDistanceMm = distance,
            RequiresAck = false,
            SafetyCategory = CncCommandSafetyCategory.Motion,
            Metadata = motion.RawText
        };
    }

    private static int ToSteps(decimal deltaMm, decimal stepsPerMm)
    {
        return (int)Math.Round(Math.Abs(deltaMm * stepsPerMm), MidpointRounding.AwayFromZero);
    }
}
