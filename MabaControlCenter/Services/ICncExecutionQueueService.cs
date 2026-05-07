using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface ICncExecutionQueueService : ICncExecutionPlanner, ICncCommandStreamer
{
    CncExecutionState ExecutionState { get; }
    IReadOnlyList<GcodeMotionCommand> LoadedMotions { get; }
    int CurrentMotionIndex { get; }
    GcodeMotionCommand? CurrentMotion { get; }
    CncPlannedCommand? CurrentPlannedCommand { get; }
    CncPlannedCommand? FailedPlannedCommand { get; }
    int CompletedCount { get; }
    string? LastInterruptionReason { get; }
    event EventHandler? ExecutionStateChanged;

    void Load(IReadOnlyList<GcodeMotionCommand> motions, string? activeJobName = null, IReadOnlyList<GcodeInterpretedCommand>? interpretedCommands = null);
    Task StartAsync(ICncControllerService controllerService);
    void Pause();
    Task ResumeAsync(ICncControllerService controllerService);
    Task StopAsync(ICncControllerService controllerService);
}
