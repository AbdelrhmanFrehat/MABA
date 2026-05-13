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
    IReadOnlyList<GcodeInterpretedCommand> InterpretedCommands { get; }
    CncStreamingSession CreatePlan(IReadOnlyList<GcodeInterpretedCommand> commands, ICncControllerService controllerService, string? activeJobName);
}

public interface ICncControllerStateMachine
{
    bool CanExecute(CncRuntimeStatus status, CncRuntimeAction action, out string? reason);
    bool CanTransition(CncRuntimeState from, CncRuntimeState to);
}

public interface ICncRuntimeCoordinator
{
    CncRuntimeStatus Current { get; }
    CncRecoveryPlan CurrentRecoveryPlan { get; }
    event EventHandler? StatusChanged;

    CncRuntimeStatus Refresh();
    bool CanExecute(CncRuntimeAction action, out string? reason);
    void SetConnectionInProgress(bool isConnecting);
    void SetBooting(bool isBooting);
    void SetLoadingJob(bool isLoading);
    void SetJobLoaded(string? activeJobName, bool isLoaded);
    void SetJobPlacementOffset(CncJobPlacementOffset placementOffset);
    void SetFraming(bool isFraming);
    void SetRecovering(bool isRecovering);
    void SetStopRequested(bool isStopping);
}

public interface ICncRecoveryPlannerService
{
    CncRecoveryPlan BuildPlan(CncRuntimeStatus status, ICncExecutionQueueService executionQueueService, ICncJobSessionService jobSessionService);
}

public interface ICncCoordinateTransformService
{
    CncCoordinateSystemState CreateState(
        decimal machineX,
        decimal machineY,
        decimal machineZ,
        CncWorkOffset? workOffset = null,
        CncJobPlacementOffset? placementOffset = null,
        CncMachineReferenceState? referenceState = null,
        CncCoordinateMode coordinateMode = CncCoordinateMode.Work);

    CncCoordinateTransformResult WorkToMachine(
        decimal workX,
        decimal workY,
        decimal workZ,
        CncCoordinateSystemState state);

    CncCoordinateTransformResult MachineToWork(
        decimal machineX,
        decimal machineY,
        decimal machineZ,
        CncCoordinateSystemState state);

    CncCoordinateTransformResult FlattenForFirmware(
        decimal rawWorkX,
        decimal rawWorkY,
        decimal rawWorkZ,
        CncCoordinateSystemState state);

    CncCoordinateTransformResult ApplyJobPlacement(
        decimal workX,
        decimal workY,
        decimal workZ,
        CncCoordinateSystemState state);

    string? ValidateBounds(
        decimal machineX,
        decimal machineY,
        decimal machineZ,
        CncMachineBounds bounds,
        CncMachineConfig config);

    CncFrameBounds ComputeFrameBounds(IReadOnlyList<GcodeMotionCommand> motions);

    string ExplainTransform(CncCoordinateTransformResult result);
}
