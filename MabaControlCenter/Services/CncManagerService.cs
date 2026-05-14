using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class CncManagerService : ICncManagerService
{
    private readonly ICncControllerService _controllerService;
    private readonly ICncExecutionQueueService _executionQueueService;
    private readonly ICncJobSessionService _jobSessionService;
    private readonly ICncRuntimeCoordinator _runtimeCoordinator;
    private readonly ICncExecutionPreflightService _preflightService;
    private readonly ICncCoordinateTransformService _coordinateTransformService;

    public CncManagerService(
        ICncControllerService controllerService,
        ICncExecutionQueueService executionQueueService,
        ICncJobSessionService jobSessionService,
        ICncRuntimeCoordinator runtimeCoordinator,
        ICncExecutionPreflightService preflightService,
        ICncCoordinateTransformService coordinateTransformService)
    {
        _controllerService = controllerService;
        _executionQueueService = executionQueueService;
        _jobSessionService = jobSessionService;
        _runtimeCoordinator = runtimeCoordinator;
        _preflightService = preflightService;
        _coordinateTransformService = coordinateTransformService;
    }

    public string Connect(string? portName)
    {
        EnsureAllowed(CncRuntimeAction.Connect);
        _runtimeCoordinator.SetConnectionInProgress(true);
        _runtimeCoordinator.SetBooting(true);
        try
        {
            _controllerService.Connect(portName ?? string.Empty);
            return "Connection established.";
        }
        finally
        {
            _runtimeCoordinator.SetBooting(false);
            _runtimeCoordinator.SetConnectionInProgress(false);
        }
    }

    public void Disconnect()
    {
        EnsureAllowed(CncRuntimeAction.Disconnect);
        _controllerService.Disconnect();
    }

    public string RefreshStatus()
    {
        EnsureAllowed(CncRuntimeAction.RefreshStatus);
        return _controllerService.RefreshStatus();
    }

    public string Unlock()
    {
        EnsureAllowed(CncRuntimeAction.Unlock);
        BeginRecovery();
        try
        {
            return _controllerService.EnableMotors();
        }
        finally
        {
            EndRecovery();
        }
    }

    public string Home()
    {
        EnsureAllowed(CncRuntimeAction.Home);
        BeginRecovery();
        try
        {
            return _controllerService.AutoHome();
        }
        finally
        {
            EndRecovery();
        }
    }

    public string Jog(string axis, decimal deltaMm)
    {
        EnsureAllowed(CncRuntimeAction.Jog);
        return _controllerService.Jog(axis, deltaMm);
    }

    public string SetWorkZeroX()
    {
        EnsureAllowed(CncRuntimeAction.SetWorkZero);
        return _controllerService.SetWorkZeroX();
    }

    public string SetWorkZeroY()
    {
        EnsureAllowed(CncRuntimeAction.SetWorkZero);
        return _controllerService.SetWorkZeroY();
    }

    public string SetWorkZeroZ()
    {
        EnsureAllowed(CncRuntimeAction.SetWorkZero);
        return _controllerService.SetWorkZeroZ();
    }

    public string SetWorkZeroXY()
    {
        EnsureAllowed(CncRuntimeAction.SetWorkZero);
        return _controllerService.SetWorkZeroXY();
    }

    public string ClearZZero()
    {
        EnsureAllowed(CncRuntimeAction.ClearWorkZero);
        return _controllerService.ClearZZero();
    }

    public string ClearWorkZero()
    {
        EnsureAllowed(CncRuntimeAction.ClearWorkZero);
        return _controllerService.ClearWorkOffset();
    }

    public CncExecutionPreflightResult EvaluatePreflight(CncExecutionPreflightRequest request)
        => _preflightService.Evaluate(request, _controllerService);

    public async Task<CncManagerOperationResult> RunAsync(CncExecutionPreflightRequest request, IReadOnlyList<GcodeMotionCommand> motions, IReadOnlyList<GcodeInterpretedCommand> commands, string? activeJobName, int totalMotionCount)
    {
        EnsureAllowed(CncRuntimeAction.Run);
        var preflight = EvaluatePreflight(request);
        if (!preflight.IsAllowed)
        {
            return new CncManagerOperationResult
            {
                Success = false,
                Error = preflight.Summary ?? "Execution preflight failed.",
                Preflight = preflight
            };
        }

        _executionQueueService.Load(motions.ToList(), activeJobName, commands.ToList());
        _jobSessionService.StartSession(totalMotionCount);
        _controllerService.RefreshStatus();
        await _executionQueueService.StartAsync(_controllerService);
        return new CncManagerOperationResult
        {
            Success = _executionQueueService.ExecutionState != CncExecutionState.Failed,
            Message = _executionQueueService.ExecutionState == CncExecutionState.Completed ? "Execution completed." : "Execution started.",
            Preflight = preflight,
            Error = _executionQueueService.ExecutionState is CncExecutionState.Alarmed or CncExecutionState.Failed or CncExecutionState.Error
                ? _executionQueueService.LastInterruptionReason
                : null
        };
    }

    public async Task<CncManagerOperationResult> RunFrameAsync(CncExecutionPreflightRequest request, IReadOnlyList<GcodeMotionCommand> frameMotions)
    {
        EnsureAllowed(CncRuntimeAction.Frame);
        var preflight = EvaluatePreflight(request);
        if (!preflight.IsAllowed)
        {
            return new CncManagerOperationResult
            {
                Success = false,
                Error = preflight.Summary ?? "Frame preflight failed.",
                Preflight = preflight
            };
        }

        _runtimeCoordinator.SetFraming(true);
        try
        {
            var coordinateState = _controllerService.CoordinateState.Clone();
            coordinateState.JobPlacementOffset = new CncJobPlacementOffset();
            var isSimulation = _controllerService.Config.DriverType == CncDriverType.Simulated;
            var canUseSafeTravelZ = _controllerService.Config.SupportsZAxis
                && (!_controllerService.Config.RequireManualZZeroForCutting || _controllerService.ReferenceState.ZReferenceValid || isSimulation);
            var safeTravelZ = canUseSafeTravelZ
                ? Math.Clamp(_controllerService.Config.SafeTravelZMm > 0m ? _controllerService.Config.SafeTravelZMm : 5m, _controllerService.Config.ZMinMm, _controllerService.Config.ZLimitMm)
                : 0m;
            var currentLine = 0;
            foreach (var motion in frameMotions.Where(motion => motion.IsExecutable))
            {
                currentLine++;
                var frameZ = canUseSafeTravelZ ? safeTravelZ : coordinateState.WorkZ;
                var transformed = _coordinateTransformService.FlattenForFirmware(motion.EndX, motion.EndY, frameZ, coordinateState);
                transformed = _coordinateTransformService.Validate(transformed, _controllerService.Bounds, _controllerService.Config);
                if (!string.IsNullOrWhiteSpace(transformed.BoundsMessage))
                    throw new InvalidOperationException(transformed.BoundsMessage);

                var planned = new CncPlannedCommand
                {
                    CommandText = $"G0 X{transformed.FinalMachineX:0.###} Y{transformed.FinalMachineY:0.###} Z{transformed.FinalMachineZ:0.###} F{_controllerService.Config.MaxRapidXyMmPerMinute:0.###}",
                    SourceLineNumber = motion.LineNumber > 0 ? motion.LineNumber : currentLine,
                    MotionType = motion.MotionType,
                    ExpectedEndX = transformed.FinalMachineX,
                    ExpectedEndY = transformed.FinalMachineY,
                    ExpectedEndZ = transformed.FinalMachineZ,
                    EstimatedDistanceMm = motion.LengthMm,
                    RequiresAck = true,
                    SafetyCategory = CncCommandSafetyCategory.Motion,
                    MotionClass = CncMotionExecutionClass.FrameMove,
                    CoordinateSpace = GcodeCoordinateSpace.Machine,
                    OriginalRawLine = motion.RawText
                };

                var ack = _controllerService.ExecutePlannedCommand(planned);
                if (!ack.Success)
                    throw new InvalidOperationException(ack.ErrorMessage ?? ack.ResponseText ?? "Machine frame failed.");
            }

            return new CncManagerOperationResult
            {
                Success = true,
                Message = "Frame execution completed.",
                Preflight = preflight
            };
        }
        finally
        {
            _runtimeCoordinator.SetFraming(false);
        }
    }

    public void Pause()
    {
        EnsureAllowed(CncRuntimeAction.Pause);
        _executionQueueService.Pause();
        _jobSessionService.PauseSession("Operator paused the CNC job session.");
    }

    public async Task<CncManagerOperationResult> ResumeAsync()
    {
        EnsureAllowed(CncRuntimeAction.Resume);
        _jobSessionService.ResumeSession("Operator resumed the CNC job session.");
        await _executionQueueService.ResumeAsync(_controllerService);
        return new CncManagerOperationResult
        {
            Success = true,
            Message = _executionQueueService.ExecutionState == CncExecutionState.Completed ? "Execution completed." : "Execution resumed."
        };
    }

    public async Task<CncManagerOperationResult> StopAsync()
    {
        EnsureAllowed(CncRuntimeAction.Stop);
        _runtimeCoordinator.SetStopRequested(true);
        try
        {
            await _executionQueueService.StopAsync(_controllerService);
            _jobSessionService.StopSession(_executionQueueService.CompletedCount, _executionQueueService.LoadedMotions.Count, _controllerService.MachineX, _controllerService.MachineY, _controllerService.MachineZ, "Operator stopped the CNC job session.");
            return new CncManagerOperationResult
            {
                Success = true,
                Message = "Execution stopped."
            };
        }
        finally
        {
            _runtimeCoordinator.SetStopRequested(false);
        }
    }

    public async Task<CncManagerOperationResult> RestartAsync(CncExecutionPreflightRequest request, IReadOnlyList<GcodeMotionCommand> motions, IReadOnlyList<GcodeInterpretedCommand> commands, string? activeJobName, int totalMotionCount)
    {
        _executionQueueService.ResetToLoadedState("Preparing job restart from the beginning.");
        return await RunAsync(request, motions, commands, activeJobName, totalMotionCount);
    }

    public async Task<CncManagerOperationResult> AbortAsync()
    {
        if (_executionQueueService.ExecutionState is CncExecutionState.Running or CncExecutionState.Paused or CncExecutionState.Stopping)
            await StopAsync();

        _executionQueueService.ResetToLoadedState("Job aborted. Loaded program remains available for restart or clear.");
        if (_jobSessionService.SessionState is not CncJobLifecycleState.Completed and not CncJobLifecycleState.Stopped)
        {
            _jobSessionService.StopSession(_executionQueueService.CompletedCount, _executionQueueService.LoadedMotions.Count, _controllerService.MachineX, _controllerService.MachineY, _controllerService.MachineZ, "Operator aborted the CNC job session.");
        }

        return new CncManagerOperationResult
        {
            Success = true,
            Message = "Job aborted. Loaded program remains available."
        };
    }

    public void BeginRecovery()
    {
        _runtimeCoordinator.SetRecovering(true);
    }

    public void EndRecovery()
    {
        _runtimeCoordinator.SetRecovering(false);
    }

    private void EnsureAllowed(CncRuntimeAction action)
    {
        var descriptor = _runtimeCoordinator.Refresh().GetActionDescriptor(action);

        if (!descriptor.IsAllowed)
            throw new InvalidOperationException(descriptor.Reason ?? $"'{action}' is not allowed right now.");
    }
}
