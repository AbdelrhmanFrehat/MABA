namespace MabaControlCenter.Models;

public enum CncRuntimeState
{
    Disconnected,
    Connecting,
    Booting,
    Locked,
    Ready,
    Homing,
    Jogging,
    LoadingJob,
    JobLoaded,
    Framing,
    Running,
    FeedHold,
    Paused,
    Stopping,
    Alarm,
    Recovering,
    ProgramComplete,
    Error
}

public enum CncControllerMode
{
    Simulation,
    RealHardware
}

public enum CncRuntimeAction
{
    Connect,
    Disconnect,
    Unlock,
    DisableMotors,
    Home,
    Jog,
    LoadJob,
    ClearLoadedJob,
    Frame,
    Run,
    Pause,
    Resume,
    Stop,
    ResetAlarm,
    RefreshStatus,
    SetWorkZero,
    ClearWorkZero,
    GoToCenter,
    UploadFirmware
}

public class CncRuntimeStatus
{
    public CncRuntimeState RuntimeState { get; set; } = CncRuntimeState.Disconnected;
    public CncDeviceState ConnectionState { get; set; } = CncDeviceState.Disconnected;
    public CncControllerMode ControllerMode { get; set; } = CncControllerMode.RealHardware;
    public bool IsConnected { get; set; }
    public bool IsLocked { get; set; }
    public bool IsAlarmed { get; set; }
    public bool IsHomed { get; set; }
    public bool HasValidReference { get; set; }
    public bool IsBusy { get; set; }
    public bool CanJog { get; set; }
    public bool CanHome { get; set; }
    public bool CanRun { get; set; }
    public bool CanPause { get; set; }
    public bool CanResume { get; set; }
    public bool CanStop { get; set; }
    public decimal PositionX { get; set; }
    public decimal PositionY { get; set; }
    public decimal PositionZ { get; set; }
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
    public CncMachineReferenceState ReferenceState { get; set; } = new();
    public string ActiveJobName { get; set; } = "No job loaded";
    public double ProgressPercent { get; set; }
    public string LastControllerMessage { get; set; } = "No controller message yet.";
    public string LastAlarmMessage { get; set; } = "No active alarm.";
    public bool LimitXTriggered { get; set; }
    public bool LimitYTriggered { get; set; }
    public bool LimitZTriggered { get; set; }
    public string? FirmwareVersion { get; set; }
    public string? ProtocolVersion { get; set; }
    public CncControllerStatusConfidence ControllerStatusConfidence { get; set; } = CncControllerStatusConfidence.Unknown;
    public CncFirmwareIdentity FirmwareIdentity { get; set; } = new();
    public CncFirmwareCompatibilityResult FirmwareCompatibility { get; set; } = new();
    public string? BlockingReason { get; set; }
    public IReadOnlyList<string> BlockingReasons { get; set; } = Array.Empty<string>();
    public CncRecoveryPlan RecoveryPlan { get; set; } = new();
    public CncRuntimeActionPolicy ActionPolicy { get; set; } = new();

    public string RuntimeStateDisplay => RuntimeState switch
    {
        CncRuntimeState.FeedHold => "Feed Hold",
        CncRuntimeState.ProgramComplete => "Program Complete",
        CncRuntimeState.LoadingJob => "Loading Job",
        CncRuntimeState.JobLoaded => "Job Loaded",
        _ => RuntimeState.ToString()
    };

    public string ControllerModeDisplay => ControllerMode == CncControllerMode.Simulation
        ? "Simulation mode"
        : "Real hardware mode";

    public string ReferenceStatusDisplay => ReferenceState.StatusText;
    public string? ReferenceWarningText => ReferenceState.WarningText;
    public string? ControllerStatusWarningText => ControllerStatusConfidence switch
    {
        CncControllerStatusConfidence.LastKnown => "Controller status is based on the last known response. Refresh status before running.",
        CncControllerStatusConfidence.Stale => "Controller status is stale. Refresh is required before running or jogging.",
        CncControllerStatusConfidence.Unknown => "Controller state is not verified yet.",
        _ => null
    };

    public CncRuntimeActionDescriptor GetActionDescriptor(CncRuntimeAction action)
        => ActionPolicy[action];
}
