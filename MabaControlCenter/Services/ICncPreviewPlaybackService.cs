using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface ICncPreviewPlaybackService
{
    IReadOnlyList<GcodeMotionCommand> LoadedMotions { get; }
    IReadOnlyList<GcodeMotionCommand> ActivePlaybackMotions { get; }
    CncPreviewSimulationState SimulationState { get; }
    int CurrentSegmentIndex { get; }
    double SegmentProgress { get; }
    decimal ToolX { get; }
    decimal ToolY { get; }
    decimal ToolZ { get; }
    double PlaybackSpeed { get; }
    CncPreviewSummary Summary { get; }
    bool IsFramePlayback { get; }
    event EventHandler? PlaybackChanged;

    void Load(IReadOnlyList<GcodeMotionCommand> motions);
    void Play();
    void PlayFrame(IReadOnlyList<GcodeMotionCommand> frameMotions);
    void Pause();
    void Stop();
    void SetPlaybackSpeed(double speed);
}
