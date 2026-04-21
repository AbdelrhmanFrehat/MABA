using MabaControlCenter.Models;
using System.Windows;

namespace MabaControlCenter.Services;

public class CncExecutionQueueService : ICncExecutionQueueService
{
    private readonly List<GcodeMotionCommand> _loadedMotions = new();
    private Task? _runnerTask;
    private bool _pauseRequested;
    private bool _stopRequested;
    private int _currentMotionIndex = -1;
    private int _completedCount;
    private CncExecutionState _executionState = CncExecutionState.Idle;
    private string? _lastInterruptionReason;

    public CncExecutionState ExecutionState => _executionState;
    public IReadOnlyList<GcodeMotionCommand> LoadedMotions => _loadedMotions;
    public int CurrentMotionIndex => _currentMotionIndex;
    public GcodeMotionCommand? CurrentMotion =>
        _currentMotionIndex >= 0 && _currentMotionIndex < _loadedMotions.Count
            ? _loadedMotions[_currentMotionIndex]
            : null;
    public int CompletedCount => _completedCount;
    public string? LastInterruptionReason => _lastInterruptionReason;
    public event EventHandler? ExecutionStateChanged;

    public void Load(IReadOnlyList<GcodeMotionCommand> motions)
    {
        _loadedMotions.Clear();
        _loadedMotions.AddRange(motions.Where(m => m.IsExecutable));
        _currentMotionIndex = -1;
        _completedCount = 0;
        _pauseRequested = false;
        _stopRequested = false;
        _lastInterruptionReason = null;
        SetState(CncExecutionState.Idle);
    }

    public Task StartAsync(ICncControllerService controllerService)
    {
        if (_executionState == CncExecutionState.Running)
            throw new InvalidOperationException("Execution is already running.");

        if (_loadedMotions.Count == 0)
            throw new InvalidOperationException("Load a parsed G-code file before starting execution.");

        if (!controllerService.IsConnected)
            throw new InvalidOperationException("Connect the CNC machine before starting execution.");

        if (!controllerService.DeviceStatus.IsResponsive)
            throw new InvalidOperationException("The CNC device is connected but has not completed protocol handshake yet.");

        if (!controllerService.MotorsEnabled)
            throw new InvalidOperationException("Enable motors before starting execution.");

        if (controllerService.MachineState is CncMachineState.Alarm or CncMachineState.Error)
            throw new InvalidOperationException(controllerService.LastFaultReason ?? "Clear the machine alarm before starting execution.");

        _pauseRequested = false;
        _stopRequested = false;
        _lastInterruptionReason = null;
        _currentMotionIndex = _completedCount;
        _runnerTask = RunQueueAsync(controllerService);
        return _runnerTask;
    }

    public void Pause()
    {
        if (_executionState == CncExecutionState.Running)
        {
            _pauseRequested = true;
            _lastInterruptionReason = "Execution paused by operator.";
            SetState(CncExecutionState.Paused);
        }
    }

    public Task ResumeAsync(ICncControllerService controllerService)
    {
        if (_executionState != CncExecutionState.Paused)
            return Task.CompletedTask;

        if (!controllerService.IsConnected)
            throw new InvalidOperationException("Reconnect the CNC machine before resuming.");

        if (!controllerService.DeviceStatus.IsResponsive)
            throw new InvalidOperationException("The CNC device is not responding to protocol acknowledgements.");

        if (!controllerService.MotorsEnabled)
            throw new InvalidOperationException("Enable motors before resuming execution.");

        _pauseRequested = false;
        _stopRequested = false;
        _lastInterruptionReason = null;
        _runnerTask = RunQueueAsync(controllerService, startIndex: _completedCount);
        return _runnerTask;
    }

    public async Task StopAsync(ICncControllerService controllerService)
    {
        _stopRequested = true;
        _pauseRequested = false;
        _lastInterruptionReason = "Execution stopped by operator.";

        if (controllerService.IsConnected)
            controllerService.Stop();

        if (_runnerTask != null)
            await _runnerTask;

        SetState(CncExecutionState.Stopped);
    }

    private async Task RunQueueAsync(ICncControllerService controllerService, int? startIndex = null)
    {
        controllerService.ConnectionLost -= OnControllerConnectionLost;
        controllerService.ConnectionLost += OnControllerConnectionLost;
        SetState(CncExecutionState.Running);
        controllerService.SetMachineState(CncMachineState.Running);
        var index = startIndex ?? _completedCount;

        try
        {
            for (; index < _loadedMotions.Count; index++)
            {
                if (!controllerService.IsConnected)
                {
                    _lastInterruptionReason = "Execution aborted because the CNC machine disconnected.";
                    controllerService.SetMachineState(CncMachineState.Alarm, _lastInterruptionReason);
                    SetState(CncExecutionState.Error);
                    return;
                }

                if (!controllerService.MotorsEnabled)
                {
                    _lastInterruptionReason = "Execution aborted because motors are disabled.";
                    controllerService.SetMachineState(CncMachineState.Alarm, _lastInterruptionReason);
                    SetState(CncExecutionState.Error);
                    return;
                }

                if (controllerService.MachineState is CncMachineState.Alarm or CncMachineState.Error)
                {
                    _lastInterruptionReason = controllerService.LastFaultReason
                                              ?? controllerService.DeviceStatus.LastProtocolError
                                              ?? "Execution aborted due to machine alarm.";
                    controllerService.SetMachineState(CncMachineState.Error, _lastInterruptionReason);
                    SetState(CncExecutionState.Error);
                    return;
                }

                if (!controllerService.DeviceStatus.IsResponsive)
                {
                    _lastInterruptionReason = "Execution aborted because the CNC device stopped acknowledging commands.";
                    controllerService.SetMachineState(CncMachineState.Error, _lastInterruptionReason);
                    SetState(CncExecutionState.Error);
                    return;
                }

                if (_stopRequested)
                {
                    controllerService.SetMachineState(CncMachineState.Stopped, _lastInterruptionReason);
                    SetState(CncExecutionState.Stopped);
                    return;
                }

                if (_pauseRequested)
                {
                    controllerService.SetMachineState(CncMachineState.Paused, _lastInterruptionReason);
                    SetState(CncExecutionState.Paused);
                    return;
                }

                _currentMotionIndex = index;
                NotifyChanged();
                var motion = _loadedMotions[index];

                var deltaX = motion.EndX - controllerService.WorkX;
                var deltaY = motion.EndY - controllerService.WorkY;
                var deltaZ = motion.EndZ - controllerService.WorkZ;

                var boundsMessage = controllerService.ValidateWorkPosition(motion.EndX, motion.EndY, motion.EndZ);
                if (boundsMessage != null)
                {
                    _lastInterruptionReason = $"Line {motion.LineNumber}: {boundsMessage}";
                    controllerService.SetMachineState(CncMachineState.Alarm, _lastInterruptionReason);
                    SetState(CncExecutionState.Error);
                    return;
                }

                if (deltaX != 0m)
                    controllerService.Jog("X", deltaX);
                if (deltaY != 0m)
                    controllerService.Jog("Y", deltaY);
                if (deltaZ != 0m)
                    controllerService.Jog("Z", deltaZ);

                _completedCount = index + 1;
                NotifyChanged();
                await Task.Delay(80);
            }

            _currentMotionIndex = _loadedMotions.Count - 1;
            _lastInterruptionReason = null;
            controllerService.SetMachineState(CncMachineState.Completed);
            SetState(CncExecutionState.Completed);
        }
        catch (Exception ex)
        {
            _lastInterruptionReason ??= ex.Message;
            controllerService.SetMachineState(CncMachineState.Error, _lastInterruptionReason);
            SetState(CncExecutionState.Error);
            throw;
        }
        finally
        {
            controllerService.ConnectionLost -= OnControllerConnectionLost;
        }
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
        _lastInterruptionReason = "Execution interrupted by serial disconnect.";
        SetState(CncExecutionState.Error);
    }
}
