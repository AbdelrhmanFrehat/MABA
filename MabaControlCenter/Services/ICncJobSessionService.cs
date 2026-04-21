using System.Collections.ObjectModel;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface ICncJobSessionService
{
    CncLoadedJobInfo? LoadedJob { get; }
    CncJobSession? CurrentSession { get; }
    CncJobSession? LastCompletedSession { get; }
    CncJobLifecycleState SessionState { get; }
    string ReadinessSummary { get; }
    string? BlockingReason { get; }
    ObservableCollection<CncOperatorEventEntry> OperatorEvents { get; }
    event EventHandler? SessionChanged;

    void LoadJob(GcodeParseResult program, string activeProfileName, string driverMode, string? jobTitle, string? sourceReference);
    void ClearLoadedJob();
    void UpdateRuntimeContext(string activeProfileName, string driverMode, string? jobTitle, string? sourceReference);
    void UpdateReadiness(bool isReady, string readinessSummary, string? blockingReason);
    void StartSession(int totalLines);
    void PauseSession(string message);
    void ResumeSession(string message);
    void StopSession(int completedLines, int totalLines, decimal x, decimal y, decimal z, string message);
    void CompleteSession(int completedLines, int totalLines, decimal x, decimal y, decimal z, string message);
    void FailSession(int completedLines, int totalLines, decimal x, decimal y, decimal z, string message);
    void InterruptSession(int completedLines, int totalLines, decimal x, decimal y, decimal z, string message);
}
