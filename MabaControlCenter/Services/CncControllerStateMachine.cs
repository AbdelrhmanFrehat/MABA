using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class CncControllerStateMachine : ICncControllerStateMachine
{
    private static readonly Dictionary<CncRuntimeAction, HashSet<CncRuntimeState>> AllowedActionStates = new()
    {
        [CncRuntimeAction.Connect] = new() { CncRuntimeState.Disconnected, CncRuntimeState.Error },
        [CncRuntimeAction.Disconnect] = new()
        {
            CncRuntimeState.Connecting, CncRuntimeState.Booting, CncRuntimeState.Locked, CncRuntimeState.Ready,
            CncRuntimeState.Homing, CncRuntimeState.Jogging, CncRuntimeState.JobLoaded, CncRuntimeState.Framing,
            CncRuntimeState.Running, CncRuntimeState.FeedHold, CncRuntimeState.Paused, CncRuntimeState.Stopping,
            CncRuntimeState.Alarm, CncRuntimeState.Recovering, CncRuntimeState.ProgramComplete, CncRuntimeState.Error
        },
        [CncRuntimeAction.Unlock] = new() { CncRuntimeState.Locked, CncRuntimeState.Alarm, CncRuntimeState.Error, CncRuntimeState.Recovering },
        [CncRuntimeAction.DisableMotors] = new() { CncRuntimeState.Ready, CncRuntimeState.JobLoaded, CncRuntimeState.ProgramComplete },
        [CncRuntimeAction.Home] = new() { CncRuntimeState.Locked, CncRuntimeState.Ready, CncRuntimeState.Recovering },
        [CncRuntimeAction.Jog] = new() { CncRuntimeState.Ready, CncRuntimeState.JobLoaded, CncRuntimeState.ProgramComplete },
        [CncRuntimeAction.LoadJob] = new() { CncRuntimeState.Disconnected, CncRuntimeState.Locked, CncRuntimeState.Ready, CncRuntimeState.JobLoaded, CncRuntimeState.ProgramComplete, CncRuntimeState.Error },
        [CncRuntimeAction.ClearLoadedJob] = new() { CncRuntimeState.Ready, CncRuntimeState.JobLoaded, CncRuntimeState.ProgramComplete, CncRuntimeState.Locked, CncRuntimeState.Disconnected, CncRuntimeState.Error },
        [CncRuntimeAction.Frame] = new() { CncRuntimeState.Ready, CncRuntimeState.JobLoaded, CncRuntimeState.ProgramComplete },
        [CncRuntimeAction.Run] = new() { CncRuntimeState.Ready, CncRuntimeState.JobLoaded, CncRuntimeState.ProgramComplete },
        [CncRuntimeAction.Pause] = new() { CncRuntimeState.Running },
        [CncRuntimeAction.Resume] = new() { CncRuntimeState.Paused, CncRuntimeState.FeedHold },
        [CncRuntimeAction.Stop] = new() { CncRuntimeState.Running, CncRuntimeState.Jogging, CncRuntimeState.Framing, CncRuntimeState.Homing, CncRuntimeState.Paused, CncRuntimeState.FeedHold, CncRuntimeState.Stopping },
        [CncRuntimeAction.ResetAlarm] = new() { CncRuntimeState.Alarm, CncRuntimeState.Error, CncRuntimeState.Recovering, CncRuntimeState.Locked },
        [CncRuntimeAction.RefreshStatus] = new() { CncRuntimeState.Locked, CncRuntimeState.Ready, CncRuntimeState.JobLoaded, CncRuntimeState.ProgramComplete, CncRuntimeState.Alarm, CncRuntimeState.Error, CncRuntimeState.Recovering, CncRuntimeState.Booting, CncRuntimeState.Connecting },
        [CncRuntimeAction.SetWorkZero] = new() { CncRuntimeState.Ready, CncRuntimeState.JobLoaded, CncRuntimeState.ProgramComplete },
        [CncRuntimeAction.GoToCenter] = new() { CncRuntimeState.Ready, CncRuntimeState.JobLoaded, CncRuntimeState.ProgramComplete },
        [CncRuntimeAction.UploadFirmware] = new() { CncRuntimeState.Disconnected, CncRuntimeState.Locked, CncRuntimeState.Ready, CncRuntimeState.JobLoaded, CncRuntimeState.ProgramComplete, CncRuntimeState.Error }
    };

    private static readonly HashSet<(CncRuntimeState From, CncRuntimeState To)> AllowedTransitions = new()
    {
        (CncRuntimeState.Disconnected, CncRuntimeState.Connecting),
        (CncRuntimeState.Connecting, CncRuntimeState.Booting),
        (CncRuntimeState.Connecting, CncRuntimeState.Locked),
        (CncRuntimeState.Connecting, CncRuntimeState.Ready),
        (CncRuntimeState.Booting, CncRuntimeState.Locked),
        (CncRuntimeState.Booting, CncRuntimeState.Ready),
        (CncRuntimeState.Booting, CncRuntimeState.Error),
        (CncRuntimeState.Locked, CncRuntimeState.Recovering),
        (CncRuntimeState.Recovering, CncRuntimeState.Ready),
        (CncRuntimeState.Recovering, CncRuntimeState.Homing),
        (CncRuntimeState.Recovering, CncRuntimeState.Alarm),
        (CncRuntimeState.Ready, CncRuntimeState.LoadingJob),
        (CncRuntimeState.LoadingJob, CncRuntimeState.JobLoaded),
        (CncRuntimeState.JobLoaded, CncRuntimeState.Framing),
        (CncRuntimeState.JobLoaded, CncRuntimeState.Running),
        (CncRuntimeState.JobLoaded, CncRuntimeState.Jogging),
        (CncRuntimeState.Ready, CncRuntimeState.Jogging),
        (CncRuntimeState.Ready, CncRuntimeState.Homing),
        (CncRuntimeState.Ready, CncRuntimeState.Running),
        (CncRuntimeState.Ready, CncRuntimeState.Alarm),
        (CncRuntimeState.Homing, CncRuntimeState.Ready),
        (CncRuntimeState.Homing, CncRuntimeState.Alarm),
        (CncRuntimeState.Jogging, CncRuntimeState.Ready),
        (CncRuntimeState.Jogging, CncRuntimeState.Alarm),
        (CncRuntimeState.Framing, CncRuntimeState.Ready),
        (CncRuntimeState.Framing, CncRuntimeState.Alarm),
        (CncRuntimeState.Running, CncRuntimeState.Paused),
        (CncRuntimeState.Running, CncRuntimeState.FeedHold),
        (CncRuntimeState.Running, CncRuntimeState.Stopping),
        (CncRuntimeState.Running, CncRuntimeState.ProgramComplete),
        (CncRuntimeState.Running, CncRuntimeState.Alarm),
        (CncRuntimeState.Paused, CncRuntimeState.Running),
        (CncRuntimeState.Paused, CncRuntimeState.Stopping),
        (CncRuntimeState.Paused, CncRuntimeState.Alarm),
        (CncRuntimeState.FeedHold, CncRuntimeState.Running),
        (CncRuntimeState.FeedHold, CncRuntimeState.Stopping),
        (CncRuntimeState.FeedHold, CncRuntimeState.Alarm),
        (CncRuntimeState.Stopping, CncRuntimeState.ProgramComplete),
        (CncRuntimeState.Stopping, CncRuntimeState.Locked),
        (CncRuntimeState.Stopping, CncRuntimeState.Ready),
        (CncRuntimeState.Stopping, CncRuntimeState.Alarm),
        (CncRuntimeState.ProgramComplete, CncRuntimeState.Ready),
        (CncRuntimeState.ProgramComplete, CncRuntimeState.JobLoaded),
        (CncRuntimeState.ProgramComplete, CncRuntimeState.Jogging),
        (CncRuntimeState.Alarm, CncRuntimeState.Recovering),
        (CncRuntimeState.Error, CncRuntimeState.Recovering)
    };

    public bool CanExecute(CncRuntimeStatus status, CncRuntimeAction action, out string? reason)
    {
        if (!AllowedActionStates.TryGetValue(action, out var states) || !states.Contains(status.RuntimeState))
        {
            reason = $"'{action}' is blocked while the machine is in '{status.RuntimeStateDisplay}'.";
            return false;
        }

        if (status.IsAlarmed && action is not (CncRuntimeAction.ResetAlarm or CncRuntimeAction.Unlock or CncRuntimeAction.Disconnect or CncRuntimeAction.RefreshStatus))
        {
            reason = status.LastAlarmMessage;
            return false;
        }

        reason = null;
        return true;
    }

    public bool CanTransition(CncRuntimeState from, CncRuntimeState to)
    {
        return from == to || AllowedTransitions.Contains((from, to));
    }
}
