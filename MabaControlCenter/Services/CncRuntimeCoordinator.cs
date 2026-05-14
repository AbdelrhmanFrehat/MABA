using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class CncRuntimeCoordinator : ICncRuntimeCoordinator
{
    private readonly ICncControllerService _controllerService;
    private readonly ICncExecutionQueueService _executionQueueService;
    private readonly ICncJobSessionService _jobSessionService;
    private readonly IActiveMachineContextService _activeMachineContextService;
    private readonly ICncControllerStateMachine _stateMachine;
    private readonly ICncRecoveryPlannerService _recoveryPlannerService;
    private readonly ICncRuntimeActionPolicy _actionPolicy;

    private bool _isConnecting;
    private bool _isBooting;
    private bool _isLoadingJob;
    private bool _isFraming;
    private bool _isRecovering;
    private bool _isStopping;
    private bool _isRefreshing;
    private string? _activeJobName;
    private CncJobPlacementOffset _jobPlacementOffset = new();

    public CncRuntimeCoordinator(
        ICncControllerService controllerService,
        ICncExecutionQueueService executionQueueService,
        ICncJobSessionService jobSessionService,
        IActiveMachineContextService activeMachineContextService,
        ICncControllerStateMachine stateMachine,
        ICncRecoveryPlannerService recoveryPlannerService,
        ICncRuntimeActionPolicy actionPolicy)
    {
        _controllerService = controllerService;
        _executionQueueService = executionQueueService;
        _jobSessionService = jobSessionService;
        _activeMachineContextService = activeMachineContextService;
        _stateMachine = stateMachine;
        _recoveryPlannerService = recoveryPlannerService;
        _actionPolicy = actionPolicy;
        Current = new CncRuntimeStatus();

        _controllerService.StateChanged += (_, _) => Refresh();
        _executionQueueService.ExecutionStateChanged += (_, _) => Refresh();
        _jobSessionService.SessionChanged += (_, _) => Refresh();
        _activeMachineContextService.ContextChanged += (_, _) => Refresh();
    }

    public CncRuntimeStatus Current { get; private set; }
    public CncRecoveryPlan CurrentRecoveryPlan => Current.RecoveryPlan;
    public event EventHandler? StatusChanged;

    public CncRuntimeStatus Refresh()
        => RefreshCore(notify: true);

    private CncRuntimeStatus RefreshCore(bool notify)
    {
        if (_isRefreshing)
            return Current;

        _isRefreshing = true;
        try
        {
        var controllerMode = _activeMachineContextService.Current.DriverType == DriverType.Simulated
            ? CncControllerMode.Simulation
            : CncControllerMode.RealHardware;

        var blockingReasons = BuildBlockingReasons(controllerMode);
        var runtimeState = ResolveRuntimeState();
        if (!_stateMachine.CanTransition(Current.RuntimeState, runtimeState))
            runtimeState = Current.RuntimeState == CncRuntimeState.Disconnected && runtimeState == CncRuntimeState.Ready
                ? CncRuntimeState.Ready
                : runtimeState;

        var loadedMotionCount = _executionQueueService.LoadedMotions.Count;
        var progressPercent = loadedMotionCount <= 0
            ? 0d
            : Math.Clamp((double)_executionQueueService.CompletedCount / loadedMotionCount * 100d, 0d, 100d);

        var status = new CncRuntimeStatus
        {
            RuntimeState = runtimeState,
            ConnectionState = _controllerService.DeviceStatus.DeviceState,
            ControllerMode = controllerMode,
            IsConnected = _controllerService.IsConnected,
            IsLocked = _controllerService.DeviceStatus.IsLocked,
            IsAlarmed = _controllerService.DeviceStatus.IsAlarmed || _controllerService.MachineState is CncMachineState.Alarm or CncMachineState.Error,
            IsHomed = _controllerService.IsHomed,
            HasValidReference = _controllerService.HasValidMachineReference,
            HasValidZReference = _controllerService.ReferenceState.ZReferenceValid,
            IsBusy = runtimeState is CncRuntimeState.Connecting or CncRuntimeState.Booting or CncRuntimeState.Homing or CncRuntimeState.Jogging or CncRuntimeState.Framing or CncRuntimeState.Running or CncRuntimeState.FeedHold or CncRuntimeState.Paused or CncRuntimeState.Stopping or CncRuntimeState.Recovering,
            MachineX = _controllerService.MachineX,
            MachineY = _controllerService.MachineY,
            MachineZ = _controllerService.MachineZ,
            PositionX = _controllerService.DeviceStatus.ReportedX ?? _controllerService.MachineX,
            PositionY = _controllerService.DeviceStatus.ReportedY ?? _controllerService.MachineY,
            PositionZ = _controllerService.DeviceStatus.ReportedZ ?? _controllerService.MachineZ,
            WorkX = _controllerService.WorkX,
            WorkY = _controllerService.WorkY,
            WorkZ = _controllerService.WorkZ,
            WorkOffsetX = _controllerService.WorkOffsetX,
            WorkOffsetY = _controllerService.WorkOffsetY,
            WorkOffsetZ = _controllerService.WorkOffsetZ,
            PlacementOffsetX = _jobPlacementOffset.X,
            PlacementOffsetY = _jobPlacementOffset.Y,
            PlacementOffsetZ = _jobPlacementOffset.Z,
            ReferenceState = _controllerService.ReferenceState.Clone(),
            ActiveJobName = _activeJobName ?? _jobSessionService.LoadedJob?.JobTitle ?? "No job loaded",
            ProgressPercent = progressPercent,
            LastControllerMessage = _controllerService.DeviceStatus.LastStatusText
                                    ?? _controllerService.DeviceStatus.LastAcknowledgement
                                    ?? "No controller message yet.",
            LastAlarmMessage = _controllerService.LastFaultReason
                               ?? _controllerService.DeviceStatus.LastProtocolError
                               ?? "No active alarm.",
            LimitXTriggered = _controllerService.DeviceStatus.LimitXTriggered,
            LimitYTriggered = _controllerService.DeviceStatus.LimitYTriggered,
            LimitZTriggered = _controllerService.DeviceStatus.LimitZTriggered,
            FirmwareVersion = _controllerService.DeviceStatus.FirmwareVersion,
            ControllerStatusConfidence = _controllerService.DeviceStatus.StatusConfidence,
            FirmwareIdentity = _controllerService.DeviceStatus.FirmwareIdentity.Clone(),
            FirmwareCompatibility = _activeMachineContextService.Current.FirmwareCompatibility.Clone(),
            ProtocolVersion = _controllerService.DeviceStatus.ProtocolVersion
                              ?? _activeMachineContextService.Current.MachineDefinition?.RuntimeBinding.FirmwareProtocol.ToString(),
            BlockingReasons = blockingReasons,
            BlockingReason = blockingReasons.FirstOrDefault()
        };

        status.RecoveryPlan = _recoveryPlannerService.BuildPlan(status, _executionQueueService, _jobSessionService);
        status.ActionPolicy = _actionPolicy.Build(status);

        status.CanJog = status.GetActionDescriptor(CncRuntimeAction.Jog).IsAllowed;
        status.CanHome = status.GetActionDescriptor(CncRuntimeAction.Home).IsAllowed;
        status.CanRun = status.GetActionDescriptor(CncRuntimeAction.Run).IsAllowed;
        status.CanPause = status.GetActionDescriptor(CncRuntimeAction.Pause).IsAllowed;
        status.CanResume = status.GetActionDescriptor(CncRuntimeAction.Resume).IsAllowed;
        status.CanStop = status.GetActionDescriptor(CncRuntimeAction.Stop).IsAllowed;

        Current = status;
        if (notify)
            StatusChanged?.Invoke(this, EventArgs.Empty);
        return Current;
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    public bool CanExecute(CncRuntimeAction action, out string? reason)
    {
        var status = RefreshCore(notify: false);
        var descriptor = status.GetActionDescriptor(action);
        reason = descriptor.Reason;
        return descriptor.IsAllowed;
    }

    public void SetConnectionInProgress(bool isConnecting)
    {
        _isConnecting = isConnecting;
        if (!isConnecting)
            _isBooting = false;
        Refresh();
    }

    public void SetBooting(bool isBooting)
    {
        _isBooting = isBooting;
        Refresh();
    }

    public void SetLoadingJob(bool isLoading)
    {
        _isLoadingJob = isLoading;
        if (isLoading)
            _activeJobName = null;
        Refresh();
    }

    public void SetJobLoaded(string? activeJobName, bool isLoaded)
    {
        _activeJobName = isLoaded ? activeJobName : null;
        if (!isLoaded)
            _isLoadingJob = false;
        Refresh();
    }

    public void SetJobPlacementOffset(CncJobPlacementOffset placementOffset)
    {
        _jobPlacementOffset = placementOffset?.Clone() ?? new CncJobPlacementOffset();
        Refresh();
    }

    public void SetFraming(bool isFraming)
    {
        _isFraming = isFraming;
        Refresh();
    }

    public void SetRecovering(bool isRecovering)
    {
        _isRecovering = isRecovering;
        Refresh();
    }

    public void SetStopRequested(bool isStopping)
    {
        _isStopping = isStopping;
        Refresh();
    }

    private CncRuntimeState ResolveRuntimeState()
    {
        if (_isConnecting)
            return CncRuntimeState.Connecting;
        if (_isBooting || (_controllerService.IsConnected && !_controllerService.DeviceStatus.IsResponsive && _controllerService.DeviceStatus.DeviceState == CncDeviceState.Unknown))
            return CncRuntimeState.Booting;
        if (!_controllerService.IsConnected)
            return CncRuntimeState.Disconnected;
        if (_isRecovering)
            return CncRuntimeState.Recovering;
        if (_controllerService.DeviceStatus.IsAlarmed || _controllerService.MachineState == CncMachineState.Alarm)
            return CncRuntimeState.Alarm;
        if (_controllerService.MachineState == CncMachineState.Error || _controllerService.DeviceStatus.DeviceState == CncDeviceState.Error)
            return CncRuntimeState.Error;
        if (_isStopping || _executionQueueService.IsStopRequested)
            return CncRuntimeState.Stopping;
        if (_controllerService.MachineState == CncMachineState.Homing || _controllerService.DeviceStatus.DeviceState == CncDeviceState.Homing)
            return CncRuntimeState.Homing;
        if (_isFraming)
            return CncRuntimeState.Framing;
        if (_executionQueueService.ExecutionState is CncExecutionState.Running or CncExecutionState.PreflightChecking or CncExecutionState.ReadyToRun)
            return CncRuntimeState.Running;
        if (_executionQueueService.ExecutionState == CncExecutionState.Paused)
            return _controllerService.DriverCapabilities.SupportsPause ? CncRuntimeState.FeedHold : CncRuntimeState.Paused;
        if (_executionQueueService.ExecutionState == CncExecutionState.Alarmed)
            return CncRuntimeState.Alarm;
        if (_isLoadingJob)
            return CncRuntimeState.LoadingJob;
        if (_executionQueueService.ExecutionState == CncExecutionState.Completed)
            return CncRuntimeState.ProgramComplete;
        if (_controllerService.MachineState == CncMachineState.Running)
            return CncRuntimeState.Jogging;
        if (_controllerService.DeviceStatus.IsLocked
            || (!_controllerService.MotorsEnabled
                && _controllerService.IsConnected
                && _executionQueueService.ExecutionState is CncExecutionState.Idle or CncExecutionState.JobLoaded or CncExecutionState.Completed or CncExecutionState.Stopped
                && _controllerService.MachineState is not (CncMachineState.Alarm or CncMachineState.Error)))
            return CncRuntimeState.Locked;
        if (_jobSessionService.LoadedJob != null || _executionQueueService.LoadedMotions.Count > 0)
            return CncRuntimeState.JobLoaded;
        return CncRuntimeState.Ready;
    }

    private List<string> BuildBlockingReasons(CncControllerMode controllerMode)
    {
        var reasons = new List<string>();

        if (!_controllerService.IsConnected)
            reasons.Add(controllerMode == CncControllerMode.Simulation
                ? "Simulation mode is selected but not connected yet."
                : "Machine is not connected.");

        if (_controllerService.DeviceStatus.IsAlarmed || _controllerService.MachineState is CncMachineState.Alarm or CncMachineState.Error)
            reasons.Add(_controllerService.LastFaultReason ?? _controllerService.DeviceStatus.LastProtocolError ?? "Machine alarm is active.");

        if (_controllerService.DeviceStatus.IsLocked)
            reasons.Add("Machine locked - unlock or home required.");

        if (controllerMode == CncControllerMode.RealHardware && !_controllerService.HasValidMachineReference)
            reasons.Add(_controllerService.ReferenceState.WarningText ?? "Machine reference is unknown.");

        if (_jobSessionService.LoadedJob == null && _executionQueueService.LoadedMotions.Count == 0)
            reasons.Add("Load a CNC job before running.");

        if (_executionQueueService.LoadedMotions.Count == 0 && _jobSessionService.LoadedJob != null)
            reasons.Add("Loaded job has no executable motion plan.");

        if (!_controllerService.MotorsEnabled && controllerMode == CncControllerMode.RealHardware)
            reasons.Add("Controller is not unlocked/ready for motion.");

        if (!_controllerService.IsHomed && controllerMode == CncControllerMode.RealHardware)
            reasons.Add("Homing required before run.");

        var compatibility = _activeMachineContextService.Current.RuntimeProfile?.CompatibilityState;
        if (compatibility is DefinitionCompatibilityState.DefinitionIncompatible or DefinitionCompatibilityState.DefinitionMissing)
            reasons.Add("Selected machine profile is missing or incompatible.");

        if (_activeMachineContextService.Current.FirmwareCompatibility.Status == CncFirmwareCompatibilityStatus.Incompatible)
            reasons.Add(_activeMachineContextService.Current.FirmwareCompatibility.Errors.FirstOrDefault() ?? "Connected firmware is incompatible with the selected machine profile.");

        if (controllerMode == CncControllerMode.RealHardware
            && _controllerService.DeviceStatus.StatusConfidence is CncControllerStatusConfidence.Unknown or CncControllerStatusConfidence.Stale)
        {
            reasons.Add(_controllerService.DeviceStatus.StatusConfidence == CncControllerStatusConfidence.Stale
                ? "Controller status is stale. Refresh is required before motion."
                : "Controller status is not verified yet.");
        }

        return reasons.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }
}
