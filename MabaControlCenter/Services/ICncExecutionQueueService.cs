using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface ICncExecutionQueueService
{
    CncExecutionState ExecutionState { get; }
    IReadOnlyList<GcodeMotionCommand> LoadedMotions { get; }
    int CurrentMotionIndex { get; }
    GcodeMotionCommand? CurrentMotion { get; }
    int CompletedCount { get; }
    string? LastInterruptionReason { get; }
    event EventHandler? ExecutionStateChanged;

    void Load(IReadOnlyList<GcodeMotionCommand> motions);
    Task StartAsync(ICncControllerService controllerService);
    void Pause();
    Task ResumeAsync(ICncControllerService controllerService);
    Task StopAsync(ICncControllerService controllerService);
}
