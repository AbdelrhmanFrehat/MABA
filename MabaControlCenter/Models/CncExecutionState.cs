namespace MabaControlCenter.Models;

public enum CncExecutionState
{
    Idle,
    JobLoaded,
    PreflightChecking,
    ReadyToRun,
    Running,
    Paused,
    Stopping,
    Stopped,
    Alarmed,
    Error,
    Failed,
    Completed
}
