namespace MabaControlCenter.Models;

public enum CncRecoveryState
{
    None,
    AlarmDetected,
    StopRequested,
    DisconnectDetected,
    ControllerReset,
    ReferenceLost,
    JobInterrupted,
    RequiresUnlock,
    RequiresRehome,
    ReadyToRecover,
    Recovering,
    RecoveryComplete,
    RecoveryFailed
}

public enum CncRecoveryAction
{
    RefreshStatus,
    UnlockController,
    Reconnect,
    ClearAlarm,
    RehomeMachine,
    ClearJob,
    ResumeJob,
    RestartJob,
    AbortJob,
    ResetWorkOffset
}

public enum CncRecoveryReason
{
    None,
    Alarm,
    Disconnect,
    ControllerReset,
    ReferenceLost,
    JobInterrupted,
    StopRequested,
    Locked,
    HomingRequired,
    ResumeBlocked,
    RecoveryFailed
}

public enum CncRecoverySeverity
{
    Info,
    Warning,
    Critical
}

public class CncRecoveryPlan
{
    public CncRecoveryState State { get; set; } = CncRecoveryState.None;
    public CncRecoveryReason Reason { get; set; } = CncRecoveryReason.None;
    public CncRecoverySeverity Severity { get; set; } = CncRecoverySeverity.Info;
    public string Summary { get; set; } = "No recovery action is required.";
    public string RequiredNextAction { get; set; } = "No action required.";
    public List<CncRecoveryAction> AllowedActions { get; set; } = new();
    public List<CncRecoveryAction> BlockedActions { get; set; } = new();
    public bool ResumeAllowed { get; set; }
    public bool RequiresRehome { get; set; }
    public bool CanTrustWorkOffset { get; set; } = true;
    public bool PreferRestartOverResume { get; set; }
    public int? FailedSourceLine { get; set; }
    public string? FailedCommandText { get; set; }
    public string? ControllerMessage { get; set; }
    public string? ResumeBlockedReason { get; set; }

    public bool HasRecovery => State != CncRecoveryState.None;
    public bool IsBlocking => State is not (CncRecoveryState.None or CncRecoveryState.RecoveryComplete);

    public string SeverityDisplay => Severity.ToString();

    public string StateDisplay => State switch
    {
        CncRecoveryState.AlarmDetected => "Alarm Detected",
        CncRecoveryState.StopRequested => "Stop Requested",
        CncRecoveryState.DisconnectDetected => "Disconnect Detected",
        CncRecoveryState.ControllerReset => "Controller Reset",
        CncRecoveryState.ReferenceLost => "Reference Lost",
        CncRecoveryState.JobInterrupted => "Job Interrupted",
        CncRecoveryState.RequiresUnlock => "Unlock Required",
        CncRecoveryState.RequiresRehome => "Re-home Required",
        CncRecoveryState.ReadyToRecover => "Ready To Recover",
        CncRecoveryState.Recovering => "Recovering",
        CncRecoveryState.RecoveryComplete => "Recovery Complete",
        CncRecoveryState.RecoveryFailed => "Recovery Failed",
        _ => "No Recovery Needed"
    };

    public bool Allows(CncRecoveryAction action) => AllowedActions.Contains(action);
}
