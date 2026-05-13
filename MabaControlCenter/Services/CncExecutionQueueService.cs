using System.Windows;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class CncExecutionQueueService : ICncExecutionQueueService
{
    private readonly ICncExecutionPlanner _executionPlanner;
    private readonly List<GcodeMotionCommand> _loadedMotions = new();
    private Task? _runnerTask;
    private bool _pauseRequested;
    private bool _stopRequested;
    private int _currentMotionIndex = -1;
    private int _completedCount;
    private decimal _streamedDistanceMm;
    private decimal _plannedDistanceMm;
    private CncExecutionState _executionState = CncExecutionState.Idle;
    private string? _lastInterruptionReason;
    private string? _activeJobName;
    private readonly List<GcodeInterpretedCommand> _interpretedCommands = new();
    private CncStreamingSession? _currentStreamingSession;
    private readonly CncStreamingDiagnostics _standaloneDiagnostics = new();

    public CncExecutionQueueService(ICncExecutionPlanner executionPlanner)
    {
        _executionPlanner = executionPlanner;
    }

    public CncExecutionState ExecutionState => _executionState;
    public IReadOnlyList<GcodeMotionCommand> LoadedMotions => _loadedMotions;
    public IReadOnlyList<GcodeMotionCommand> PlannedMotions => _executionPlanner.PlannedMotions;
    public IReadOnlyList<GcodeInterpretedCommand> InterpretedCommands => _interpretedCommands;
    public bool HasLoadedPlan => _executionPlanner.HasLoadedPlan;
    public bool IsStreaming => StreamingState == CncStreamingState.Streaming;
    public bool IsPauseRequested => _pauseRequested;
    public bool IsStopRequested => _stopRequested;
    public CncStreamingState StreamingState => _currentStreamingSession?.State ?? CncStreamingState.Idle;
    public CncStreamingSession? CurrentStreamingSession => _currentStreamingSession;
    public CncStreamingDiagnostics Diagnostics => _currentStreamingSession?.Diagnostics ?? _standaloneDiagnostics;
    public int CurrentMotionIndex => _currentMotionIndex;
    public GcodeMotionCommand? CurrentMotion => ResolveCurrentMotion();
    public CncPlannedCommand? CurrentPlannedCommand => _currentStreamingSession?.CurrentCommand;
    public CncPlannedCommand? FailedPlannedCommand => _currentStreamingSession?.FailedCommand;
    public int CompletedCount => _completedCount;
    public string? LastInterruptionReason => _lastInterruptionReason;
    public event EventHandler? ExecutionStateChanged;

    public void Load(IReadOnlyList<GcodeMotionCommand> motions, string? activeJobName = null, IReadOnlyList<GcodeInterpretedCommand>? interpretedCommands = null)
    {
        _loadedMotions.Clear();
        _loadedMotions.AddRange(motions.Where(m => m.IsExecutable));
        _interpretedCommands.Clear();
        if (interpretedCommands != null)
            _interpretedCommands.AddRange(interpretedCommands);
        _activeJobName = string.IsNullOrWhiteSpace(activeJobName) ? "Loaded CNC Job" : activeJobName;
        _currentMotionIndex = -1;
        _completedCount = 0;
        _pauseRequested = false;
        _stopRequested = false;
        _streamedDistanceMm = 0m;
        _plannedDistanceMm = 0m;
        _lastInterruptionReason = null;
        _currentStreamingSession = _loadedMotions.Count == 0
            ? null
            : new CncStreamingSession
            {
                ActiveJobName = _activeJobName,
                State = CncStreamingState.Idle,
                LastMessage = "Job loaded and ready for planning."
            };
        Diagnostics.Events.Clear();
        if (_currentStreamingSession != null)
            _currentStreamingSession.Diagnostics.AddEvent("Job motions loaded into the execution pipeline.");
        SetState(_loadedMotions.Count == 0 ? CncExecutionState.Idle : CncExecutionState.JobLoaded);
    }

    public CncStreamingSession CreatePlan(IReadOnlyList<GcodeInterpretedCommand> commands, ICncControllerService controllerService, string? activeJobName)
    {
        return _executionPlanner.CreatePlan(commands, controllerService, activeJobName);
    }

    public Task StartAsync(ICncControllerService controllerService)
    {
        if (_executionState == CncExecutionState.Running)
            throw new InvalidOperationException("Execution is already running.");

        if (_loadedMotions.Count == 0)
            throw new InvalidOperationException("Load a parsed G-code file before starting execution.");

        _pauseRequested = false;
        _stopRequested = false;
        _lastInterruptionReason = null;
        _streamedDistanceMm = 0m;
        _completedCount = 0;
        _currentMotionIndex = -1;

        var planSource = _interpretedCommands.Count > 0
            ? _interpretedCommands
            : BuildFallbackInterpretedCommands();
        _currentStreamingSession = _executionPlanner.CreatePlan(planSource, controllerService, _activeJobName);
        _plannedDistanceMm = _currentStreamingSession.PlannedCommands.Sum(command => command.EstimatedDistanceMm);
        _currentStreamingSession.State = CncStreamingState.Preflight;
        _currentStreamingSession.LastMessage = "Planner complete. Running execution preflight.";
        _currentStreamingSession.Diagnostics.AddEvent(_currentStreamingSession.LastMessage);
        SetState(CncExecutionState.PreflightChecking);

        var blockingReason = GetPreflightFailure(controllerService);
        if (blockingReason != null)
        {
            FailCurrentSession(blockingReason, markAlarm: false, markFailedCommand: null);
            return Task.CompletedTask;
        }

        _currentStreamingSession.State = CncStreamingState.Streaming;
        _currentStreamingSession.LastMessage = "Preflight passed. Starting command stream.";
        _currentStreamingSession.Diagnostics.AddEvent(_currentStreamingSession.LastMessage);
        SetState(CncExecutionState.ReadyToRun);
        _runnerTask = RunStreamingAsync(controllerService, startIndex: 0);
        return _runnerTask;
    }

    private IReadOnlyList<GcodeInterpretedCommand> BuildFallbackInterpretedCommands()
    {
        return _loadedMotions.Select(motion => new GcodeInterpretedCommand
        {
            SourceLineNumber = motion.LineNumber,
            RawLine = motion.RawText,
            SanitizedLine = motion.RawText,
            Units = motion.Units,
            DistanceMode = motion.DistanceMode,
            Plane = motion.Plane,
            MotionMode = motion.MotionType switch
            {
                GcodeMotionType.Rapid => GcodeModalMotionMode.Rapid,
                GcodeMotionType.Linear => GcodeModalMotionMode.Linear,
                GcodeMotionType.ArcClockwise => GcodeModalMotionMode.ArcClockwise,
                GcodeMotionType.ArcCounterClockwise => GcodeModalMotionMode.ArcCounterClockwise,
                _ => GcodeModalMotionMode.Linear
            },
            HasMotion = true,
            EmitsControllerCommand = true,
            StartX = motion.StartX,
            StartY = motion.StartY,
            StartZ = motion.StartZ,
            EndX = motion.EndX,
            EndY = motion.EndY,
            EndZ = motion.EndZ,
            FeedRateMmPerMinute = motion.FeedRate,
            ArcOffsetI = motion.ArcOffsetI,
            ArcOffsetJ = motion.ArcOffsetJ,
            ArcOffsetK = motion.ArcOffsetK,
            ArcCenterX = motion.ArcCenterX,
            ArcCenterY = motion.ArcCenterY,
            ArcCenterZ = motion.ArcCenterZ,
            ArcRadiusMm = motion.ArcRadiusMm,
            ArcLengthMm = motion.ArcLengthMm,
            CoordinateSpace = GcodeCoordinateSpace.Work
        }).ToList();
    }

    public void Pause()
    {
        if (_executionState != CncExecutionState.Running)
            return;

        _pauseRequested = true;
        _lastInterruptionReason = "Pause requested. Waiting for the in-flight command to finish.";
        if (_currentStreamingSession != null)
        {
            _currentStreamingSession.LastMessage = _lastInterruptionReason;
            _currentStreamingSession.Diagnostics.AddEvent(_lastInterruptionReason);
        }

        NotifyChanged();
    }

    public Task ResumeAsync(ICncControllerService controllerService)
    {
        if (_executionState != CncExecutionState.Paused)
            return Task.CompletedTask;

        if (_currentStreamingSession == null)
            throw new InvalidOperationException("No paused streaming session is available to resume.");

        if (!controllerService.IsConnected)
            throw new InvalidOperationException("Reconnect the CNC machine before resuming.");

        if (controllerService.DeviceStatus.IsAlarmed || controllerService.MachineState is CncMachineState.Alarm or CncMachineState.Error)
            throw new InvalidOperationException("Resume blocked until the controller alarm is recovered.");

        _pauseRequested = false;
        _stopRequested = false;
        _lastInterruptionReason = null;
        _currentStreamingSession.State = CncStreamingState.Streaming;
        _currentStreamingSession.LastMessage = "Streaming resumed from the next unsent command.";
        _currentStreamingSession.Diagnostics.AddEvent(_currentStreamingSession.LastMessage);
        _runnerTask = RunStreamingAsync(controllerService, startIndex: _completedCount);
        return _runnerTask;
    }

    public async Task StopAsync(ICncControllerService controllerService)
    {
        _stopRequested = true;
        _pauseRequested = false;
        _lastInterruptionReason = "Execution stopped by operator.";

        if (_currentStreamingSession != null)
        {
            _currentStreamingSession.State = CncStreamingState.Stopping;
            _currentStreamingSession.LastMessage = _lastInterruptionReason;
            _currentStreamingSession.Diagnostics.AddEvent(_lastInterruptionReason);
        }

        SetState(CncExecutionState.Stopping);

        if (controllerService.IsConnected)
            controllerService.Stop();

        if (_runnerTask != null)
            await _runnerTask;

        if (_currentStreamingSession != null && _currentStreamingSession.State is not (CncStreamingState.Alarmed or CncStreamingState.Failed))
        {
            _currentStreamingSession.State = CncStreamingState.Cancelled;
            _currentStreamingSession.LastMessage = "Streaming cancelled. Unsent commands were dropped.";
            _currentStreamingSession.Diagnostics.AddEvent(_currentStreamingSession.LastMessage);
        }

        SetState(_currentStreamingSession?.State == CncStreamingState.Alarmed
            ? CncExecutionState.Alarmed
            : CncExecutionState.Stopped);
    }

    public void ResetToLoadedState(string? message = null)
    {
        _pauseRequested = false;
        _stopRequested = false;
        _currentMotionIndex = -1;
        _completedCount = 0;
        _streamedDistanceMm = 0m;
        _plannedDistanceMm = 0m;
        _lastInterruptionReason = message;

        Diagnostics.Events.Clear();
        _currentStreamingSession = _loadedMotions.Count == 0
            ? null
            : new CncStreamingSession
            {
                ActiveJobName = _activeJobName ?? "Loaded CNC Job",
                State = CncStreamingState.Idle,
                LastMessage = string.IsNullOrWhiteSpace(message)
                    ? "Execution session reset. Job is loaded and ready for a new run."
                    : message
            };

        if (_currentStreamingSession != null)
            _currentStreamingSession.Diagnostics.AddEvent(_currentStreamingSession.LastMessage ?? "Execution session reset.");

        SetState(_loadedMotions.Count == 0 ? CncExecutionState.Idle : CncExecutionState.JobLoaded);
    }

    private async Task RunStreamingAsync(ICncControllerService controllerService, int startIndex)
    {
        if (_currentStreamingSession == null)
            throw new InvalidOperationException("Execution plan is missing.");

        controllerService.ConnectionLost -= OnControllerConnectionLost;
        controllerService.ConnectionLost += OnControllerConnectionLost;
        SetState(CncExecutionState.Running);
        _currentStreamingSession.State = CncStreamingState.Streaming;
        _currentStreamingSession.Diagnostics.AddEvent($"Streaming started at command index {startIndex + 1}.");

        try
        {
            for (var index = startIndex; index < _currentStreamingSession.PlannedCommands.Count; index++)
            {
                if (!controllerService.IsConnected)
                {
                    FailCurrentSession("Execution aborted because the CNC machine disconnected.", markAlarm: true, markFailedCommand: CurrentPlannedCommand);
                    return;
                }

                if (controllerService.DeviceStatus.IsAlarmed || controllerService.MachineState is CncMachineState.Alarm or CncMachineState.Error)
                {
                    FailCurrentSession(controllerService.LastFaultReason ?? controllerService.DeviceStatus.LastProtocolError ?? "Execution aborted due to machine alarm.", markAlarm: true, markFailedCommand: CurrentPlannedCommand);
                    return;
                }

                if (_stopRequested)
                {
                    _currentStreamingSession.State = CncStreamingState.Cancelled;
                    _currentStreamingSession.LastMessage = "Streaming cancelled before sending the next command.";
                    _currentStreamingSession.Diagnostics.AddEvent(_currentStreamingSession.LastMessage);
                    SetState(CncExecutionState.Stopped);
                    return;
                }

                if (_pauseRequested)
                {
                    _currentStreamingSession.State = CncStreamingState.Paused;
                    _currentStreamingSession.LastMessage = "Streaming paused. No new commands will be sent until resume.";
                    _currentStreamingSession.Diagnostics.AddEvent(_currentStreamingSession.LastMessage);
                    controllerService.SetMachineState(CncMachineState.Paused, _currentStreamingSession.LastMessage);
                    SetState(CncExecutionState.Paused);
                    return;
                }

                var command = _currentStreamingSession.PlannedCommands[index];
                PrepareCurrentCommand(index, command);

                var ack = controllerService.ExecutePlannedCommand(command);
                _currentStreamingSession.Diagnostics.SentCommands++;
                _currentStreamingSession.Diagnostics.LastControllerResponse = ack.ResponseText;

                if (ack.Success)
                {
                    _currentStreamingSession.Diagnostics.AcknowledgedCommands++;
                    _currentStreamingSession.Diagnostics.RecordAck(ack.RoundTripMilliseconds, ack.ResponseText);
                    _completedCount = index + 1;
                    _streamedDistanceMm += command.EstimatedDistanceMm;
                    UpdateProgress(command, ack.ResponseText);
                    continue;
                }

                if (ack.IsTimeout)
                    _currentStreamingSession.Diagnostics.TimeoutCount++;

                _currentStreamingSession.Diagnostics.FailedCommands++;
                _currentStreamingSession.Diagnostics.FailedSourceLine = ack.SourceLineNumber ?? command.SourceLineNumber;
                _currentStreamingSession.Diagnostics.FailedCommandText = command.CommandText;

                var failureMessage = ack.ErrorMessage
                                     ?? ack.ResponseText
                                     ?? $"Controller failed on source line {command.SourceLineNumber}.";
                FailCurrentSession(failureMessage, ack.IsAlarm, command);
                return;
            }

            _currentStreamingSession.State = CncStreamingState.Completed;
            _currentStreamingSession.ProgressPercent = 100d;
            _currentStreamingSession.LastMessage = "Streaming completed successfully.";
            _currentStreamingSession.Diagnostics.AddEvent(_currentStreamingSession.LastMessage);
            _currentStreamingSession.CurrentCommand = null;
            SetState(CncExecutionState.Completed);
        }
        catch (Exception ex)
        {
            FailCurrentSession(ex.Message, controllerService.DeviceStatus.IsAlarmed, CurrentPlannedCommand);
            throw;
        }
        finally
        {
            controllerService.ConnectionLost -= OnControllerConnectionLost;
        }
    }

    private string? GetPreflightFailure(ICncControllerService controllerService)
    {
        if (_currentStreamingSession == null || _currentStreamingSession.PlannedCommands.Count == 0)
            return "No executable CNC motions are loaded for streaming.";
        if (!controllerService.IsConnected)
            return "Connect the CNC machine before starting execution.";
        if (!controllerService.DeviceStatus.IsResponsive)
            return "The CNC device is connected but not responding to protocol acknowledgements.";
        if (controllerService.DeviceStatus.IsAlarmed || controllerService.MachineState is CncMachineState.Alarm or CncMachineState.Error)
            return controllerService.LastFaultReason ?? controllerService.DeviceStatus.LastProtocolError ?? "Controller alarm blocks execution.";
        if (!controllerService.MotorsEnabled)
            return "Unlock or enable the machine before starting execution.";
        if (!string.Equals(controllerService.ConnectedPort, "SIMULATION", StringComparison.OrdinalIgnoreCase)
            && !controllerService.HasValidMachineReference)
            return controllerService.ReferenceState.WarningText ?? "Machine reference is not valid for execution.";

        return null;
    }

    private void PrepareCurrentCommand(int index, CncPlannedCommand command)
    {
        if (_currentStreamingSession == null)
            return;

        _currentStreamingSession.CurrentCommandIndex = index;
        _currentStreamingSession.CurrentCommand = command;
        _currentMotionIndex = ResolveMotionIndex(command.SourceLineNumber, index);
        _currentStreamingSession.LastMessage = $"Streaming {command.ProgressLabel}: {command.CommandText}";
        _currentStreamingSession.Diagnostics.AddEvent(_currentStreamingSession.LastMessage);
        NotifyChanged();
    }

    private void UpdateProgress(CncPlannedCommand command, string responseText)
    {
        if (_currentStreamingSession == null)
            return;

        _currentStreamingSession.CompletedCommands = _completedCount;
        _currentStreamingSession.StreamedDistanceMm = _streamedDistanceMm;
        _currentStreamingSession.ProgressPercent = CalculateProgressPercent();
        _currentStreamingSession.LastMessage = $"Acknowledged {command.ProgressLabel}: {responseText}";
        _currentStreamingSession.Diagnostics.AddEvent(_currentStreamingSession.LastMessage);
        NotifyChanged();
    }

    private double CalculateProgressPercent()
    {
        if (_currentStreamingSession == null || _currentStreamingSession.TotalCommands == 0)
            return 0d;

        var commandProgress = (_completedCount / (double)_currentStreamingSession.TotalCommands) * 100d;
        if (_plannedDistanceMm <= 0m)
            return commandProgress;

        var distanceProgress = (double)(_streamedDistanceMm / _plannedDistanceMm) * 100d;
        return Math.Clamp((commandProgress + distanceProgress) / 2d, 0d, 100d);
    }

    private void FailCurrentSession(string message, bool markAlarm, CncPlannedCommand? markFailedCommand)
    {
        _lastInterruptionReason = message;

        if (_currentStreamingSession != null)
        {
            _currentStreamingSession.FailedCommand = markFailedCommand;
            _currentStreamingSession.LastMessage = message;
            _currentStreamingSession.State = markAlarm ? CncStreamingState.Alarmed : CncStreamingState.Failed;
            _currentStreamingSession.Diagnostics.AddEvent(message);

            if (markFailedCommand != null)
            {
                _currentStreamingSession.Diagnostics.FailedSourceLine = markFailedCommand.SourceLineNumber;
                _currentStreamingSession.Diagnostics.FailedCommandText = markFailedCommand.CommandText;
            }
        }

        SetState(markAlarm ? CncExecutionState.Alarmed : CncExecutionState.Failed);
    }

    private GcodeMotionCommand? ResolveCurrentMotion()
    {
        if (_loadedMotions.Count == 0)
            return null;

        if (CurrentPlannedCommand != null)
            return _loadedMotions.LastOrDefault(m => m.LineNumber == CurrentPlannedCommand.SourceLineNumber);

        return _currentMotionIndex >= 0 && _currentMotionIndex < _loadedMotions.Count
            ? _loadedMotions[_currentMotionIndex]
            : null;
    }

    private int ResolveMotionIndex(int sourceLineNumber, int fallbackIndex)
    {
        for (var i = 0; i < _loadedMotions.Count; i++)
        {
            if (_loadedMotions[i].LineNumber == sourceLineNumber)
                return i;
        }

        return Math.Clamp(fallbackIndex, -1, _loadedMotions.Count - 1);
    }

    private void SetState(CncExecutionState state)
    {
        _executionState = state;
        NotifyChanged();
    }

    private void NotifyChanged()
    {
        if (Application.Current?.Dispatcher == null || Application.Current.Dispatcher.CheckAccess())
        {
            ExecutionStateChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        Application.Current.Dispatcher.Invoke(() => ExecutionStateChanged?.Invoke(this, EventArgs.Empty));
    }

    private void OnControllerConnectionLost(object? sender, EventArgs e)
    {
        _stopRequested = true;
        _pauseRequested = false;
        FailCurrentSession("Execution interrupted by serial disconnect.", markAlarm: true, markFailedCommand: CurrentPlannedCommand);
    }
}
