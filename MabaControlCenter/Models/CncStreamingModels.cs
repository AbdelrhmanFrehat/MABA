using System.Collections.ObjectModel;

namespace MabaControlCenter.Models;

public enum CncStreamingState
{
    Idle,
    Planning,
    Preflight,
    Streaming,
    Paused,
    FeedHold,
    Stopping,
    Alarmed,
    Completed,
    Failed,
    Cancelled
}

public enum CncCommandSafetyCategory
{
    Motion,
    RealtimeStop,
    Recovery,
    Status,
    System
}

public class CncPlannedCommand
{
    public string CommandText { get; set; } = string.Empty;
    public int SourceLineNumber { get; set; }
    public GcodeMotionType MotionType { get; set; }
    public decimal? ExpectedEndX { get; set; }
    public decimal? ExpectedEndY { get; set; }
    public decimal? ExpectedEndZ { get; set; }
    public decimal EstimatedDistanceMm { get; set; }
    public bool RequiresAck { get; set; } = true;
    public bool IsRealtimeCommand { get; set; }
    public CncCommandSafetyCategory SafetyCategory { get; set; } = CncCommandSafetyCategory.Motion;
    public string? Metadata { get; set; }
    public int SequenceIndex { get; set; }
    public GcodeCoordinateSpace CoordinateSpace { get; set; } = GcodeCoordinateSpace.Work;
    public decimal? FeedRateMmPerMinute { get; set; }
    public GcodeSpindleState? SpindleState { get; set; }
    public string? OriginalRawLine { get; set; }
    public string ProgressLabel => $"Line {SourceLineNumber}";
}

public class CncControllerAckResult
{
    public bool Success { get; set; }
    public bool IsAlarm { get; set; }
    public bool IsTimeout { get; set; }
    public bool IsCancelled { get; set; }
    public string ResponseText { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime AcknowledgedAt { get; set; } = DateTime.UtcNow;
    public double RoundTripMilliseconds { get; set; }
    public int? SourceLineNumber { get; set; }
}

public class CncStreamingDiagnostics
{
    private readonly List<double> _ackSamples = new();

    public int SentCommands { get; set; }
    public int AcknowledgedCommands { get; set; }
    public int FailedCommands { get; set; }
    public int TimeoutCount { get; set; }
    public int RetryCount { get; set; }
    public int? FailedSourceLine { get; set; }
    public string? FailedCommandText { get; set; }
    public string? LastControllerResponse { get; set; }
    public double LastAckMilliseconds { get; set; }
    public double AverageAckMilliseconds => _ackSamples.Count == 0 ? 0d : _ackSamples.Average();
    public ObservableCollection<string> Events { get; } = new();

    public void RecordAck(double roundTripMilliseconds, string response)
    {
        LastAckMilliseconds = roundTripMilliseconds;
        LastControllerResponse = response;
        _ackSamples.Add(roundTripMilliseconds);
    }

    public void AddEvent(string message)
    {
        Events.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
    }
}

public class CncStreamingSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString("N");
    public CncStreamingState State { get; set; } = CncStreamingState.Idle;
    public List<CncPlannedCommand> PlannedCommands { get; set; } = new();
    public int CurrentCommandIndex { get; set; } = -1;
    public int CompletedCommands { get; set; }
    public CncPlannedCommand? CurrentCommand { get; set; }
    public CncPlannedCommand? FailedCommand { get; set; }
    public double ProgressPercent { get; set; }
    public decimal StreamedDistanceMm { get; set; }
    public string ActiveJobName { get; set; } = "No job loaded";
    public CncStreamingDiagnostics Diagnostics { get; set; } = new();
    public string? LastMessage { get; set; }
    public int TotalCommands => PlannedCommands.Count;
}
