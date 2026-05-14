using System.Globalization;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class CncExecutionPlannerService : ICncExecutionPlanner
{
    private readonly IActiveMachineContextService _activeMachineContextService;
    private readonly ICncCoordinateTransformService _coordinateTransformService;
    private readonly List<GcodeMotionCommand> _plannedMotions = new();
    private readonly List<GcodeInterpretedCommand> _interpretedCommands = new();

    public CncExecutionPlannerService(
        IActiveMachineContextService activeMachineContextService,
        ICncCoordinateTransformService coordinateTransformService)
    {
        _activeMachineContextService = activeMachineContextService;
        _coordinateTransformService = coordinateTransformService;
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

        if (_interpretedCommands.Count == 0)
        {
            session.State = CncStreamingState.Preflight;
            session.LastMessage = "No interpreted commands were available for planning.";
            session.Diagnostics.AddEvent(session.LastMessage);
            return session;
        }

        var driverType = _activeMachineContextService.Current.DriverType;
        var firmwareProtocol = _activeMachineContextService.Current.MachineDefinition?.RuntimeBinding.FirmwareProtocol;
        var firmwareIdentity = _activeMachineContextService.Current.FirmwareIdentity;
        var config = controllerService.Config;
        var isSimulation = driverType == DriverType.Simulated;
        var supportsZ = config.SupportsZAxis;
        var safeTravelZ = ResolveSafeTravelZ(config);
        var canUseSafeTravelZ = supportsZ
                                && (!config.RequireManualZZeroForCutting || controllerService.ReferenceState.ZReferenceValid || isSimulation);
        var planningState = controllerService.CoordinateState.Clone();
        var cursor = new PlannerCursor
        {
            WorkX = planningState.WorkX,
            WorkY = planningState.WorkY,
            WorkZ = planningState.WorkZ
        };

        var expandedCommands = new List<GcodeInterpretedCommand>();
        var motionCommands = _interpretedCommands.Where(command => command.HasMotion && command.MotionType != null).ToList();
        var firstMotion = motionCommands.FirstOrDefault();
        var firstCutIndex = FindFirstCuttingCommandIndex(_interpretedCommands, supportsZ, safeTravelZ);
        var autoManageSpindle = firstCutIndex >= 0
                                && (!firmwareIdentity.IsKnown || firmwareIdentity.Capabilities.SupportsSpindleOnOff)
                                && !HasExplicitSpindleOnBefore(_interpretedCommands, firstCutIndex);

        if (firstMotion != null)
        {
            ApplyPlacementOffset(planningState, firstMotion);
            AppendStartSequence(expandedCommands, cursor, firstMotion, supportsZ, canUseSafeTravelZ, safeTravelZ, autoManageSpindle);
        }

        for (var index = 0; index < _interpretedCommands.Count; index++)
        {
            var command = CloneCommand(_interpretedCommands[index]);
            ApplyPlacementOffset(planningState, command);

            if (command.IsUnsupported || command.BlocksExecution)
            {
                expandedCommands.Add(command);
                continue;
            }

            if (command.IsSpindleChange)
            {
                command.PlannedMotionClass = CncMotionExecutionClass.SpindleCommand;
                expandedCommands.Add(command);
                cursor.SpindleState = command.SpindleState;
                continue;
            }

            if (!command.HasMotion || command.MotionType == null)
            {
                expandedCommands.Add(command);
                continue;
            }

            if (autoManageSpindle
                && cursor.SpindleState != GcodeSpindleState.Clockwise
                && IsCuttingCommand(command, supportsZ, canUseSafeTravelZ, safeTravelZ))
            {
                expandedCommands.Add(CreateSyntheticSpindleCommand(command.SourceLineNumber, GcodeSpindleState.Clockwise));
                cursor.SpindleState = GcodeSpindleState.Clockwise;
            }

            foreach (var expanded in ExpandMotionCommand(command, cursor, supportsZ, canUseSafeTravelZ, safeTravelZ))
                expandedCommands.Add(expanded);

            cursor.WorkX = command.EndX;
            cursor.WorkY = command.EndY;
            cursor.WorkZ = command.EndZ;
        }

        if (firstMotion != null)
            AppendEndSequence(expandedCommands, cursor, motionCommands.Last(), config, supportsZ, canUseSafeTravelZ, safeTravelZ);

        var sequence = 0;
        foreach (var command in expandedCommands)
        {
            foreach (var planned in CreatePlannedCommands(command, controllerService, driverType, firmwareProtocol, firmwareIdentity, config, planningState))
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

    private IEnumerable<GcodeInterpretedCommand> ExpandMotionCommand(
        GcodeInterpretedCommand command,
        PlannerCursor cursor,
        bool supportsZ,
        bool canUseSafeTravelZ,
        decimal safeTravelZ)
    {
        if (command.MotionType != GcodeMotionType.Rapid || !supportsZ || !canUseSafeTravelZ)
        {
            command.PlannedMotionClass = ClassifyCommand(command, supportsZ, canUseSafeTravelZ, safeTravelZ);
            yield return command;
            yield break;
        }

        var xyChanged = HasXyTravel(command.StartX, command.StartY, command.EndX, command.EndY);
        var currentZ = cursor.WorkZ;
        var targetZ = command.EndZ;
        var requiresSafeTravel = xyChanged && (currentZ < safeTravelZ || targetZ < safeTravelZ);

        if (!requiresSafeTravel)
        {
            command.PlannedMotionClass = CncMotionExecutionClass.RapidTravel;
            yield return command;
            yield break;
        }

        var travelZ = Math.Max(safeTravelZ, currentZ);
        if (currentZ < travelZ)
        {
            yield return CreateSyntheticMotion(
                command.SourceLineNumber,
                cursor.WorkX,
                cursor.WorkY,
                currentZ,
                cursor.WorkX,
                cursor.WorkY,
                travelZ,
                GcodeModalMotionMode.Rapid,
                CncMotionExecutionClass.SafeZLift);
        }

        if (xyChanged)
        {
            yield return CreateSyntheticMotion(
                command.SourceLineNumber,
                cursor.WorkX,
                cursor.WorkY,
                travelZ,
                command.EndX,
                command.EndY,
                travelZ,
                GcodeModalMotionMode.Rapid,
                CncMotionExecutionClass.RapidTravel);
        }

        if (targetZ != travelZ)
        {
            yield return CreateSyntheticMotion(
                command.SourceLineNumber,
                command.EndX,
                command.EndY,
                travelZ,
                command.EndX,
                command.EndY,
                targetZ,
                GcodeModalMotionMode.Rapid,
                targetZ < travelZ ? CncMotionExecutionClass.SafeZLower : CncMotionExecutionClass.Retract);
        }
    }

    private void AppendStartSequence(
        List<GcodeInterpretedCommand> output,
        PlannerCursor cursor,
        GcodeInterpretedCommand firstMotion,
        bool supportsZ,
        bool canUseSafeTravelZ,
        decimal safeTravelZ,
        bool autoManageSpindle)
    {
        var needsTravel = HasXyTravel(cursor.WorkX, cursor.WorkY, firstMotion.StartX, firstMotion.StartY);
        var travelZ = supportsZ ? Math.Max(safeTravelZ, cursor.WorkZ) : cursor.WorkZ;

        if (supportsZ && canUseSafeTravelZ && cursor.WorkZ < travelZ)
        {
            output.Add(CreateSyntheticMotion(
                firstMotion.SourceLineNumber,
                cursor.WorkX,
                cursor.WorkY,
                cursor.WorkZ,
                cursor.WorkX,
                cursor.WorkY,
                travelZ,
                GcodeModalMotionMode.Rapid,
                CncMotionExecutionClass.SafeZLift));
            cursor.WorkZ = travelZ;
        }

        if (needsTravel)
        {
            output.Add(CreateSyntheticMotion(
                firstMotion.SourceLineNumber,
                cursor.WorkX,
                cursor.WorkY,
                cursor.WorkZ,
                firstMotion.StartX,
                firstMotion.StartY,
                cursor.WorkZ,
                GcodeModalMotionMode.Rapid,
                CncMotionExecutionClass.RapidTravel));
            cursor.WorkX = firstMotion.StartX;
            cursor.WorkY = firstMotion.StartY;
        }

        if (supportsZ && canUseSafeTravelZ && cursor.WorkZ != firstMotion.StartZ)
        {
            if (autoManageSpindle
                && cursor.SpindleState != GcodeSpindleState.Clockwise
                && IsCuttingCommand(firstMotion, supportsZ, canUseSafeTravelZ, safeTravelZ)
                && firstMotion.StartZ < cursor.WorkZ)
            {
                output.Add(CreateSyntheticSpindleCommand(firstMotion.SourceLineNumber, GcodeSpindleState.Clockwise));
                cursor.SpindleState = GcodeSpindleState.Clockwise;
            }

            output.Add(CreateSyntheticMotion(
                firstMotion.SourceLineNumber,
                cursor.WorkX,
                cursor.WorkY,
                cursor.WorkZ,
                firstMotion.StartX,
                firstMotion.StartY,
                firstMotion.StartZ,
                GcodeModalMotionMode.Rapid,
                firstMotion.StartZ < cursor.WorkZ ? CncMotionExecutionClass.SafeZLower : CncMotionExecutionClass.Retract));
            cursor.WorkZ = firstMotion.StartZ;
        }
    }

    private void AppendEndSequence(
        List<GcodeInterpretedCommand> output,
        PlannerCursor cursor,
        GcodeInterpretedCommand lastMotion,
        CncMachineConfig config,
        bool supportsZ,
        bool canUseSafeTravelZ,
        decimal safeTravelZ)
    {
        if (supportsZ && canUseSafeTravelZ && cursor.WorkZ < safeTravelZ)
        {
            output.Add(CreateSyntheticMotion(
                lastMotion.SourceLineNumber,
                cursor.WorkX,
                cursor.WorkY,
                cursor.WorkZ,
                cursor.WorkX,
                cursor.WorkY,
                safeTravelZ,
                GcodeModalMotionMode.Rapid,
                CncMotionExecutionClass.SafeZLift));
            cursor.WorkZ = safeTravelZ;
        }

        if (cursor.SpindleState == GcodeSpindleState.Clockwise)
        {
            output.Add(CreateSyntheticSpindleCommand(lastMotion.SourceLineNumber, GcodeSpindleState.Off));
            cursor.SpindleState = GcodeSpindleState.Off;
        }

        if (config.ParkXMm.HasValue || config.ParkYMm.HasValue || config.ParkZMm.HasValue)
        {
            var parkX = config.ParkXMm ?? cursor.WorkX;
            var parkY = config.ParkYMm ?? cursor.WorkY;
            var parkZ = config.ParkZMm ?? cursor.WorkZ;

            if (supportsZ && canUseSafeTravelZ && cursor.WorkZ < parkZ)
            {
                output.Add(CreateSyntheticMotion(
                    lastMotion.SourceLineNumber,
                    cursor.WorkX,
                    cursor.WorkY,
                    cursor.WorkZ,
                    cursor.WorkX,
                    cursor.WorkY,
                    parkZ,
                    GcodeModalMotionMode.Rapid,
                    CncMotionExecutionClass.Retract));
                cursor.WorkZ = parkZ;
            }

            if (cursor.WorkX != parkX || cursor.WorkY != parkY)
            {
                output.Add(CreateSyntheticMotion(
                    lastMotion.SourceLineNumber,
                    cursor.WorkX,
                    cursor.WorkY,
                    cursor.WorkZ,
                    parkX,
                    parkY,
                    cursor.WorkZ,
                    GcodeModalMotionMode.Rapid,
                    CncMotionExecutionClass.RecoveryMove));
                cursor.WorkX = parkX;
                cursor.WorkY = parkY;
            }

            if (supportsZ && canUseSafeTravelZ && cursor.WorkZ != parkZ)
            {
                output.Add(CreateSyntheticMotion(
                    lastMotion.SourceLineNumber,
                    cursor.WorkX,
                    cursor.WorkY,
                    cursor.WorkZ,
                    cursor.WorkX,
                    cursor.WorkY,
                    parkZ,
                    GcodeModalMotionMode.Rapid,
                    parkZ > cursor.WorkZ ? CncMotionExecutionClass.Retract : CncMotionExecutionClass.SafeZLower));
                cursor.WorkZ = parkZ;
            }
        }
    }

    private IEnumerable<CncPlannedCommand> CreatePlannedCommands(
        GcodeInterpretedCommand command,
        ICncControllerService controllerService,
        DriverType driverType,
        FirmwareProtocol? firmwareProtocol,
        CncFirmwareIdentity firmwareIdentity,
        CncMachineConfig config,
        CncCoordinateSystemState planningState)
    {
        if (command.IsUnsupported || command.BlocksExecution)
            yield break;

        var knownFirmware = firmwareIdentity.IsKnown;
        var supportsSpindle = !knownFirmware || firmwareIdentity.Capabilities.SupportsSpindleOnOff;
        var supportsNativeArcs = !knownFirmware
                                 ? firmwareProtocol == FirmwareProtocol.MabaProtocol || driverType == DriverType.Simulated
                                 : firmwareIdentity.Capabilities.SupportsG2G3;

        if (command.IsSpindleChange)
        {
            if (!supportsSpindle)
                yield break;

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
                MotionClass = CncMotionExecutionClass.SpindleCommand,
                IsSynthetic = command.IsSynthetic,
                OriginalRawLine = command.RawLine
            };
            yield break;
        }

        if (!command.HasMotion || command.MotionType == null)
            yield break;

        ApplyPlacementOffset(planningState, command);
        var machineTarget = _coordinateTransformService.FlattenForFirmware(command.EndX, command.EndY, command.EndZ, planningState);
        var motionClass = command.PlannedMotionClass != CncMotionExecutionClass.None
            ? command.PlannedMotionClass
            : ClassifyCommand(
                command,
                config.SupportsZAxis,
                !config.RequireManualZZeroForCutting || controllerService.ReferenceState.ZReferenceValid || driverType == DriverType.Simulated,
                ResolveSafeTravelZ(config));
        var effectiveFeed = ResolveEffectiveFeed(command, config, motionClass);

        if (driverType == DriverType.Simulated || firmwareProtocol == FirmwareProtocol.MabaProtocol)
        {
            if (command.MotionType is GcodeMotionType.ArcClockwise or GcodeMotionType.ArcCounterClockwise)
            {
                if (!supportsNativeArcs)
                {
                    foreach (var segmented in CreateLegacyCommands(command, config, planningState, motionClass, effectiveFeed))
                        yield return segmented;
                    yield break;
                }

                yield return new CncPlannedCommand
                {
                    CommandText = string.Create(
                        CultureInfo.InvariantCulture,
                        $"{(command.MotionType == GcodeMotionType.ArcClockwise ? "G2" : "G3")} X{machineTarget.FinalMachineX:0.###} Y{machineTarget.FinalMachineY:0.###} I{(command.ArcOffsetI ?? 0m):0.###} J{(command.ArcOffsetJ ?? 0m):0.###} F{effectiveFeed:0.###}"),
                    SourceLineNumber = command.SourceLineNumber,
                    MotionType = command.MotionType.Value,
                    ExpectedEndX = machineTarget.FinalMachineX,
                    ExpectedEndY = machineTarget.FinalMachineY,
                    ExpectedEndZ = machineTarget.FinalMachineZ,
                    EstimatedDistanceMm = command.ArcLengthMm ?? ToMotion(command).LengthMm,
                    RequiresAck = true,
                    SafetyCategory = CncCommandSafetyCategory.Motion,
                    Metadata = command.RawLine,
                    CoordinateSpace = GcodeCoordinateSpace.Machine,
                    FeedRateMmPerMinute = effectiveFeed,
                    MotionClass = motionClass,
                    IsSynthetic = command.IsSynthetic,
                    OriginalRawLine = command.RawLine
                };
                yield break;
            }

            yield return new CncPlannedCommand
            {
                CommandText = string.Create(
                    CultureInfo.InvariantCulture,
                    $"{(command.MotionType == GcodeMotionType.Rapid ? "G0" : "G1")} X{machineTarget.FinalMachineX:0.###} Y{machineTarget.FinalMachineY:0.###} Z{machineTarget.FinalMachineZ:0.###} F{effectiveFeed:0.###}"),
                SourceLineNumber = command.SourceLineNumber,
                MotionType = command.MotionType.Value,
                ExpectedEndX = machineTarget.FinalMachineX,
                ExpectedEndY = machineTarget.FinalMachineY,
                ExpectedEndZ = machineTarget.FinalMachineZ,
                EstimatedDistanceMm = ToMotion(command).LengthMm,
                RequiresAck = true,
                SafetyCategory = CncCommandSafetyCategory.Motion,
                Metadata = command.RawLine,
                CoordinateSpace = GcodeCoordinateSpace.Machine,
                FeedRateMmPerMinute = effectiveFeed,
                MotionClass = motionClass,
                IsSynthetic = command.IsSynthetic,
                OriginalRawLine = command.RawLine
            };
            yield break;
        }

        foreach (var legacyCommand in CreateLegacyCommands(command, config, planningState, motionClass, effectiveFeed))
            yield return legacyCommand;
    }

    private IEnumerable<CncPlannedCommand> CreateLegacyCommands(
        GcodeInterpretedCommand command,
        CncMachineConfig config,
        CncCoordinateSystemState coordinateState,
        CncMotionExecutionClass motionClass,
        decimal effectiveFeed)
    {
        if (command.MotionType is GcodeMotionType.ArcClockwise or GcodeMotionType.ArcCounterClockwise)
        {
            var sampled = GcodeMotionGeometry.SamplePath(ToMotion(command), minimumSegments: 60);
            for (var i = 0; i < sampled.Count - 1; i++)
            {
                var start = sampled[i];
                var end = sampled[i + 1];
                foreach (var segmented in CreateLegacyLinearCommands(command, config, coordinateState, start.X, start.Y, start.Z, end.X, end.Y, end.Z, motionClass, effectiveFeed))
                    yield return segmented;
            }

            yield break;
        }

        foreach (var linear in CreateLegacyLinearCommands(command, config, coordinateState, command.StartX, command.StartY, command.StartZ, command.EndX, command.EndY, command.EndZ, motionClass, effectiveFeed))
            yield return linear;
    }

    private IEnumerable<CncPlannedCommand> CreateLegacyLinearCommands(
        GcodeInterpretedCommand command,
        CncMachineConfig config,
        CncCoordinateSystemState coordinateState,
        decimal startX,
        decimal startY,
        decimal startZ,
        decimal endX,
        decimal endY,
        decimal endZ,
        CncMotionExecutionClass motionClass,
        decimal effectiveFeed)
    {
        var startMachine = _coordinateTransformService.FlattenForFirmware(startX, startY, startZ, coordinateState);
        var endMachine = _coordinateTransformService.FlattenForFirmware(endX, endY, endZ, coordinateState);
        var deltaX = endMachine.FinalMachineX - startMachine.FinalMachineX;
        var deltaY = endMachine.FinalMachineY - startMachine.FinalMachineY;
        var deltaZ = endMachine.FinalMachineZ - startMachine.FinalMachineZ;
        var xSteps = ToSteps(deltaX, config.XStepsPerMm);
        var ySteps = ToSteps(deltaY, config.YStepsPerMm);
        var zSteps = ToSteps(deltaZ, config.ZStepsPerMm);

        if (xSteps > 0 && ySteps > 0 && zSteps == 0)
        {
            yield return new CncPlannedCommand
            {
                CommandText = $"XY,{(deltaX >= 0m ? xSteps : -xSteps)},{(deltaY >= 0m ? ySteps : -ySteps)}",
                SourceLineNumber = command.SourceLineNumber,
                MotionType = GcodeMotionType.Linear,
                ExpectedEndX = endMachine.FinalMachineX,
                ExpectedEndY = endMachine.FinalMachineY,
                ExpectedEndZ = startMachine.FinalMachineZ,
                EstimatedDistanceMm = (decimal)Math.Sqrt((double)((deltaX * deltaX) + (deltaY * deltaY))),
                RequiresAck = false,
                SafetyCategory = CncCommandSafetyCategory.Motion,
                Metadata = command.RawLine,
                CoordinateSpace = GcodeCoordinateSpace.Machine,
                FeedRateMmPerMinute = effectiveFeed,
                MotionClass = motionClass,
                IsSynthetic = command.IsSynthetic,
                OriginalRawLine = command.RawLine
            };
            yield break;
        }

        if (xSteps > 0)
            yield return BuildLegacyAxisCommand("x", command, endMachine.FinalMachineX, startMachine.FinalMachineY, startMachine.FinalMachineZ, xSteps, deltaX >= 0m, Math.Abs(deltaX), motionClass, effectiveFeed);
        if (ySteps > 0)
            yield return BuildLegacyAxisCommand("y", command, endMachine.FinalMachineX, endMachine.FinalMachineY, startMachine.FinalMachineZ, ySteps, deltaY >= 0m, Math.Abs(deltaY), motionClass, effectiveFeed);
        if (zSteps > 0)
            yield return BuildLegacyAxisCommand("z", command, endMachine.FinalMachineX, endMachine.FinalMachineY, endMachine.FinalMachineZ, zSteps, deltaZ >= 0m, Math.Abs(deltaZ), motionClass, effectiveFeed);
    }

    private static CncPlannedCommand BuildLegacyAxisCommand(
        string axis,
        GcodeInterpretedCommand command,
        decimal endX,
        decimal endY,
        decimal endZ,
        int steps,
        bool positive,
        decimal distance,
        CncMotionExecutionClass motionClass,
        decimal effectiveFeed)
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
            CoordinateSpace = GcodeCoordinateSpace.Machine,
            FeedRateMmPerMinute = effectiveFeed,
            MotionClass = motionClass,
            IsSynthetic = command.IsSynthetic,
            OriginalRawLine = command.RawLine
        };
    }

    private static GcodeInterpretedCommand CloneCommand(GcodeInterpretedCommand command)
    {
        return new GcodeInterpretedCommand
        {
            SourceLineNumber = command.SourceLineNumber,
            RawLine = command.RawLine,
            SanitizedLine = command.SanitizedLine,
            CommentText = command.CommentText,
            IsIgnored = command.IsIgnored,
            IsUnsupported = command.IsUnsupported,
            BlocksExecution = command.BlocksExecution,
            DiagnosticMessage = command.DiagnosticMessage,
            Units = command.Units,
            DistanceMode = command.DistanceMode,
            Plane = command.Plane,
            MotionMode = command.MotionMode,
            HasMotion = command.HasMotion,
            EmitsControllerCommand = command.EmitsControllerCommand,
            IsSpindleChange = command.IsSpindleChange,
            SpindleState = command.SpindleState,
            SpindleSpeed = command.SpindleSpeed,
            ToolNumber = command.ToolNumber,
            StartX = command.StartX,
            StartY = command.StartY,
            StartZ = command.StartZ,
            EndX = command.EndX,
            EndY = command.EndY,
            EndZ = command.EndZ,
            FeedRateMmPerMinute = command.FeedRateMmPerMinute,
            ArcOffsetI = command.ArcOffsetI,
            ArcOffsetJ = command.ArcOffsetJ,
            ArcOffsetK = command.ArcOffsetK,
            ArcCenterX = command.ArcCenterX,
            ArcCenterY = command.ArcCenterY,
            ArcCenterZ = command.ArcCenterZ,
            ArcRadiusMm = command.ArcRadiusMm,
            ArcLengthMm = command.ArcLengthMm,
            CoordinateSpace = command.CoordinateSpace,
            PlannedMotionClass = command.PlannedMotionClass,
            IsSynthetic = command.IsSynthetic,
            ModalStateAfterLine = command.ModalStateAfterLine.Clone()
        };
    }

    private static GcodeInterpretedCommand CreateSyntheticMotion(
        int sourceLineNumber,
        decimal startX,
        decimal startY,
        decimal startZ,
        decimal endX,
        decimal endY,
        decimal endZ,
        GcodeModalMotionMode motionMode,
        CncMotionExecutionClass motionClass)
    {
        return new GcodeInterpretedCommand
        {
            SourceLineNumber = sourceLineNumber,
            RawLine = $"SYNTHETIC:{motionClass}",
            SanitizedLine = $"SYNTHETIC:{motionClass}",
            HasMotion = true,
            EmitsControllerCommand = true,
            MotionMode = motionMode,
            Units = GcodeUnitMode.Millimeters,
            DistanceMode = GcodeDistanceMode.Absolute,
            Plane = GcodePlane.XY,
            StartX = startX,
            StartY = startY,
            StartZ = startZ,
            EndX = endX,
            EndY = endY,
            EndZ = endZ,
            CoordinateSpace = GcodeCoordinateSpace.Work,
            PlannedMotionClass = motionClass,
            IsSynthetic = true
        };
    }

    private static GcodeInterpretedCommand CreateSyntheticSpindleCommand(int sourceLineNumber, GcodeSpindleState spindleState)
    {
        return new GcodeInterpretedCommand
        {
            SourceLineNumber = sourceLineNumber,
            RawLine = spindleState == GcodeSpindleState.Clockwise ? "M3" : "M5",
            SanitizedLine = spindleState == GcodeSpindleState.Clockwise ? "M3" : "M5",
            IsSpindleChange = true,
            EmitsControllerCommand = true,
            SpindleState = spindleState,
            PlannedMotionClass = CncMotionExecutionClass.SpindleCommand,
            IsSynthetic = true
        };
    }

    private static int FindFirstCuttingCommandIndex(IReadOnlyList<GcodeInterpretedCommand> commands, bool supportsZ, decimal safeTravelZ)
    {
        for (var index = 0; index < commands.Count; index++)
        {
            if (IsCuttingCommand(commands[index], supportsZ, true, safeTravelZ))
                return index;
        }

        return -1;
    }

    private static bool HasExplicitSpindleOnBefore(IReadOnlyList<GcodeInterpretedCommand> commands, int beforeIndex)
    {
        for (var index = 0; index < beforeIndex; index++)
        {
            if (commands[index].IsSpindleChange && commands[index].SpindleState == GcodeSpindleState.Clockwise)
                return true;
        }

        return false;
    }

    private static bool IsCuttingCommand(GcodeInterpretedCommand command, bool supportsZ, bool canUseSafeTravelZ, decimal safeTravelZ)
    {
        if (!command.HasMotion || command.MotionType == null)
            return false;

        if (command.MotionType == GcodeMotionType.Rapid)
            return false;

        if (!supportsZ)
            return HasXyTravel(command.StartX, command.StartY, command.EndX, command.EndY);

        if (!canUseSafeTravelZ)
            return command.StartZ != command.EndZ
                   || command.StartZ != 0m
                   || command.EndZ != 0m
                   || HasXyTravel(command.StartX, command.StartY, command.EndX, command.EndY);

        return command.StartZ < safeTravelZ
               || command.EndZ < safeTravelZ
               || (HasXyTravel(command.StartX, command.StartY, command.EndX, command.EndY)
                   && command.MotionType is GcodeMotionType.Linear or GcodeMotionType.ArcClockwise or GcodeMotionType.ArcCounterClockwise);
    }

    private static CncMotionExecutionClass ClassifyCommand(GcodeInterpretedCommand command, bool supportsZ, bool canUseSafeTravelZ, decimal safeTravelZ)
    {
        if (command.IsSpindleChange)
            return CncMotionExecutionClass.SpindleCommand;

        if (!command.HasMotion || command.MotionType == null)
            return CncMotionExecutionClass.None;

        var xyTravel = HasXyTravel(command.StartX, command.StartY, command.EndX, command.EndY);
        var zDelta = command.EndZ - command.StartZ;

        if (command.MotionType == GcodeMotionType.Rapid)
        {
            if (supportsZ && canUseSafeTravelZ && !xyTravel && zDelta > 0m)
                return CncMotionExecutionClass.SafeZLift;
            if (supportsZ && canUseSafeTravelZ && !xyTravel && zDelta < 0m)
                return command.EndZ < safeTravelZ ? CncMotionExecutionClass.SafeZLower : CncMotionExecutionClass.Retract;
            return CncMotionExecutionClass.RapidTravel;
        }

        if (!xyTravel && zDelta < 0m)
            return CncMotionExecutionClass.DrillPlunge;
        if (!xyTravel && zDelta > 0m)
            return CncMotionExecutionClass.Retract;
        if (xyTravel || command.MotionType is GcodeMotionType.ArcClockwise or GcodeMotionType.ArcCounterClockwise)
            return CncMotionExecutionClass.CuttingMove;

        return CncMotionExecutionClass.CuttingMove;
    }

    private static decimal ResolveSafeTravelZ(CncMachineConfig config)
    {
        var safe = config.SafeTravelZMm > 0m ? config.SafeTravelZMm : 5m;
        return Math.Clamp(safe, config.ZMinMm, config.ZLimitMm);
    }

    private static decimal ResolveEffectiveFeed(GcodeInterpretedCommand command, CncMachineConfig config, CncMotionExecutionClass motionClass)
    {
        var requested = command.FeedRateMmPerMinute.GetValueOrDefault(0m);
        var fallback = motionClass switch
        {
            CncMotionExecutionClass.RapidTravel => config.MaxRapidXyMmPerMinute,
            CncMotionExecutionClass.SafeZLift => config.MaxFeedZMmPerMinute,
            CncMotionExecutionClass.SafeZLower => config.MaxPlungeZMmPerMinute,
            CncMotionExecutionClass.DrillPlunge => config.MaxPlungeZMmPerMinute,
            CncMotionExecutionClass.Retract => config.MaxFeedZMmPerMinute,
            CncMotionExecutionClass.RecoveryMove => config.MaxRapidXyMmPerMinute,
            _ => config.MaxFeedXyMmPerMinute
        };

        if (fallback <= 0m)
            fallback = 300m;

        if (requested <= 0m)
            return fallback;

        decimal cap = fallback;
        var xyTravel = HasXyTravel(command.StartX, command.StartY, command.EndX, command.EndY);
        var zDelta = command.EndZ - command.StartZ;

        if (motionClass == CncMotionExecutionClass.CuttingMove && !xyTravel)
            cap = zDelta < 0m ? config.MaxPlungeZMmPerMinute : config.MaxFeedZMmPerMinute;
        else if (motionClass == CncMotionExecutionClass.CuttingMove && zDelta != 0m)
            cap = MinPositive(config.MaxFeedXyMmPerMinute, zDelta < 0m ? config.MaxPlungeZMmPerMinute : config.MaxFeedZMmPerMinute);

        if (cap <= 0m)
            return requested;

        return Math.Min(requested, cap);
    }

    private static decimal MinPositive(decimal first, decimal second)
    {
        if (first <= 0m)
            return second;
        if (second <= 0m)
            return first;
        return Math.Min(first, second);
    }

    private static bool HasXyTravel(decimal startX, decimal startY, decimal endX, decimal endY)
    {
        return startX != endX || startY != endY;
    }

    private static void ApplyPlacementOffset(CncCoordinateSystemState planningState, GcodeInterpretedCommand command)
    {
        planningState.JobPlacementOffset = new CncJobPlacementOffset
        {
            X = command.ModalStateAfterLine.Coordinates.PlacementOffsetX,
            Y = command.ModalStateAfterLine.Coordinates.PlacementOffsetY,
            Z = command.ModalStateAfterLine.Coordinates.PlacementOffsetZ
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

    private sealed class PlannerCursor
    {
        public decimal WorkX { get; set; }
        public decimal WorkY { get; set; }
        public decimal WorkZ { get; set; }
        public GcodeSpindleState SpindleState { get; set; } = GcodeSpindleState.Off;
    }
}
