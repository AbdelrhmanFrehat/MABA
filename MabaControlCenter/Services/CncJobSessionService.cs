using System.Collections.ObjectModel;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class CncJobSessionService : ICncJobSessionService
{
    private CncLoadedJobInfo? _loadedJob;
    private CncJobSession? _currentSession;
    private CncJobSession? _lastCompletedSession;
    private string _readinessSummary = "Load a G-code job to prepare an operator session.";
    private string? _blockingReason;

    public CncLoadedJobInfo? LoadedJob => _loadedJob;
    public CncJobSession? CurrentSession => _currentSession;
    public CncJobSession? LastCompletedSession => _lastCompletedSession;
    public CncJobLifecycleState SessionState => _currentSession?.SessionState ?? (_loadedJob == null ? CncJobLifecycleState.NoJob : CncJobLifecycleState.Loaded);
    public string ReadinessSummary => _readinessSummary;
    public string? BlockingReason => _blockingReason;
    public ObservableCollection<CncOperatorEventEntry> OperatorEvents { get; } = new();
    public event EventHandler? SessionChanged;

    public void LoadJob(GcodeParseResult program, string activeProfileName, string driverMode, string? jobTitle, string? sourceReference)
    {
        _loadedJob = new CncLoadedJobInfo
        {
            FileName = program.FileName,
            FilePath = program.FilePath,
            JobTitle = string.IsNullOrWhiteSpace(jobTitle) ? program.FileName : jobTitle,
            SourceReference = string.IsNullOrWhiteSpace(sourceReference) ? "Standalone file" : sourceReference,
            MotionLineCount = program.Motions.Count,
            ActiveProfileName = activeProfileName,
            DriverMode = driverMode,
            LoadedAt = DateTime.Now
        };

        _currentSession = new CncJobSession
        {
            SessionState = CncJobLifecycleState.Loaded,
            LastAction = "Job Loaded",
            ResultMessage = "Job loaded and waiting for readiness checks.",
            TotalLines = _loadedJob.MotionLineCount
        };

        _readinessSummary = "Loaded job is being prepared for operator run.";
        _blockingReason = null;
        AddOperatorEvent("Info", $"Job loaded: {_loadedJob.JobTitle} ({_loadedJob.MotionLineCount} motion lines).");
        NotifyChanged();
    }

    public void ClearLoadedJob()
    {
        _loadedJob = null;
        _currentSession = null;
        _readinessSummary = "Load a G-code job to prepare an operator session.";
        _blockingReason = null;
        AddOperatorEvent("Info", "Loaded job cleared.");
        NotifyChanged();
    }

    public void UpdateRuntimeContext(string activeProfileName, string driverMode, string? jobTitle, string? sourceReference)
    {
        if (_loadedJob != null)
        {
            _loadedJob.ActiveProfileName = activeProfileName;
            _loadedJob.DriverMode = driverMode;
            if (!string.IsNullOrWhiteSpace(jobTitle))
                _loadedJob.JobTitle = jobTitle;
            if (!string.IsNullOrWhiteSpace(sourceReference))
                _loadedJob.SourceReference = sourceReference;
        }

        NotifyChanged();
    }

    public void UpdateReadiness(bool isReady, string readinessSummary, string? blockingReason)
    {
        _readinessSummary = readinessSummary;
        _blockingReason = blockingReason;

        if (_loadedJob != null && _currentSession != null && _currentSession.SessionState is CncJobLifecycleState.Loaded or CncJobLifecycleState.Ready)
        {
            _currentSession.SessionState = isReady ? CncJobLifecycleState.Ready : CncJobLifecycleState.Loaded;
            _currentSession.LastAction = isReady ? "Ready To Run" : "Waiting For Requirements";
            _currentSession.ResultMessage = isReady ? readinessSummary : blockingReason ?? readinessSummary;
        }

        NotifyChanged();
    }

    public void StartSession(int totalLines)
    {
        EnsureLoadedJob();
        _currentSession ??= new CncJobSession();
        if (!_currentSession.StartedAt.HasValue)
            _currentSession.StartedAt = DateTime.Now;

        _currentSession.EndedAt = null;
        _currentSession.InterruptedAt = null;
        _currentSession.TotalLines = totalLines;
        _currentSession.SessionState = CncJobLifecycleState.Running;
        _currentSession.LastAction = "Session Started";
        _currentSession.ResultMessage = "Operator session is running.";
        AddOperatorEvent("Info", "Operator started the CNC job session.");
        NotifyChanged();
    }

    public void PauseSession(string message)
    {
        EnsureSession();
        _currentSession!.SessionState = CncJobLifecycleState.Paused;
        _currentSession.LastAction = "Paused";
        _currentSession.ResultMessage = message;
        AddOperatorEvent("Info", message);
        NotifyChanged();
    }

    public void ResumeSession(string message)
    {
        EnsureSession();
        _currentSession!.SessionState = CncJobLifecycleState.Running;
        _currentSession.LastAction = "Resumed";
        _currentSession.ResultMessage = message;
        AddOperatorEvent("Info", message);
        NotifyChanged();
    }

    public void StopSession(int completedLines, int totalLines, decimal x, decimal y, decimal z, string message)
    {
        EnsureSession();
        FinalizeSession(CncJobLifecycleState.Stopped, completedLines, totalLines, x, y, z, message, interrupted: true, severity: "Warning");
    }

    public void CompleteSession(int completedLines, int totalLines, decimal x, decimal y, decimal z, string message)
    {
        EnsureSession();
        FinalizeSession(CncJobLifecycleState.Completed, completedLines, totalLines, x, y, z, message, interrupted: false, severity: "Info");
    }

    public void FailSession(int completedLines, int totalLines, decimal x, decimal y, decimal z, string message)
    {
        EnsureSession();
        FinalizeSession(CncJobLifecycleState.Failed, completedLines, totalLines, x, y, z, message, interrupted: true, severity: "Error");
    }

    public void InterruptSession(int completedLines, int totalLines, decimal x, decimal y, decimal z, string message)
    {
        EnsureSession();
        FinalizeSession(CncJobLifecycleState.Interrupted, completedLines, totalLines, x, y, z, message, interrupted: true, severity: "Warning");
    }

    private void FinalizeSession(CncJobLifecycleState state, int completedLines, int totalLines, decimal x, decimal y, decimal z, string message, bool interrupted, string severity)
    {
        _currentSession!.SessionState = state;
        _currentSession.CompletedLines = completedLines;
        _currentSession.TotalLines = totalLines;
        _currentSession.LastKnownX = x;
        _currentSession.LastKnownY = y;
        _currentSession.LastKnownZ = z;
        _currentSession.LastAction = state.ToString();
        _currentSession.ResultMessage = message;
        _currentSession.EndedAt = interrupted ? null : DateTime.Now;
        _currentSession.InterruptedAt = interrupted ? DateTime.Now : null;
        _lastCompletedSession = CloneSession(_currentSession);
        AddOperatorEvent(severity, message);
        NotifyChanged();
    }

    private void AddOperatorEvent(string severity, string message)
    {
        OperatorEvents.Insert(0, new CncOperatorEventEntry
        {
            Timestamp = DateTime.Now,
            Severity = severity,
            Message = message
        });
    }

    private static CncJobSession CloneSession(CncJobSession session)
    {
        return new CncJobSession
        {
            SessionId = session.SessionId,
            SessionState = session.SessionState,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            InterruptedAt = session.InterruptedAt,
            LastAction = session.LastAction,
            ResultMessage = session.ResultMessage,
            CompletedLines = session.CompletedLines,
            TotalLines = session.TotalLines,
            LastKnownX = session.LastKnownX,
            LastKnownY = session.LastKnownY,
            LastKnownZ = session.LastKnownZ
        };
    }

    private void EnsureLoadedJob()
    {
        if (_loadedJob == null)
            throw new InvalidOperationException("Load a CNC job before starting an operator session.");
    }

    private void EnsureSession()
    {
        if (_currentSession == null)
            throw new InvalidOperationException("No CNC job session is active.");
    }

    private void NotifyChanged()
    {
        SessionChanged?.Invoke(this, EventArgs.Empty);
    }
}
