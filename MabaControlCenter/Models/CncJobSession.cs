namespace MabaControlCenter.Models;

public class CncJobSession
{
    public Guid SessionId { get; set; } = Guid.NewGuid();
    public CncJobLifecycleState SessionState { get; set; } = CncJobLifecycleState.NoJob;
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public DateTime? InterruptedAt { get; set; }
    public string LastAction { get; set; } = "Idle";
    public string ResultMessage { get; set; } = "No session started.";
    public int CompletedLines { get; set; }
    public int TotalLines { get; set; }
    public decimal LastKnownX { get; set; }
    public decimal LastKnownY { get; set; }
    public decimal LastKnownZ { get; set; }

    public TimeSpan? Duration => StartedAt.HasValue && (EndedAt ?? InterruptedAt).HasValue
        ? (EndedAt ?? InterruptedAt) - StartedAt
        : null;
}
