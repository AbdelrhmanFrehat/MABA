using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface ICncCommandStreamer
{
    bool IsStreaming { get; }
    bool IsPauseRequested { get; }
    bool IsStopRequested { get; }
    CncStreamingState StreamingState { get; }
    CncStreamingSession? CurrentStreamingSession { get; }
    CncStreamingDiagnostics Diagnostics { get; }
}

public interface ICncExecutionPlanner
{
    bool HasLoadedPlan { get; }
    IReadOnlyList<GcodeMotionCommand> PlannedMotions { get; }
    CncStreamingSession CreatePlan(IReadOnlyList<GcodeMotionCommand> motions, ICncControllerService controllerService, string? activeJobName);
}

public interface ICncControllerStateMachine
{
    bool CanExecute(CncRuntimeStatus status, CncRuntimeAction action, out string? reason);
    bool CanTransition(CncRuntimeState from, CncRuntimeState to);
}

public interface ICncRuntimeCoordinator
{
    CncRuntimeStatus Current { get; }
    event EventHandler? StatusChanged;

    CncRuntimeStatus Refresh();
    bool CanExecute(CncRuntimeAction action, out string? reason);
    void SetConnectionInProgress(bool isConnecting);
    void SetBooting(bool isBooting);
    void SetLoadingJob(bool isLoading);
    void SetJobLoaded(string? activeJobName, bool isLoaded);
    void SetFraming(bool isFraming);
    void SetRecovering(bool isRecovering);
    void SetStopRequested(bool isStopping);
}
