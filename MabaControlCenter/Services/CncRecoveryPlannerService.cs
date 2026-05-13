using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class CncRecoveryPlannerService : ICncRecoveryPlannerService
{
    public CncRecoveryPlan BuildPlan(CncRuntimeStatus status, ICncExecutionQueueService executionQueueService, ICncJobSessionService jobSessionService)
    {
        var plan = new CncRecoveryPlan
        {
            ControllerMessage = status.LastControllerMessage,
            FailedSourceLine = executionQueueService.FailedPlannedCommand?.SourceLineNumber ?? executionQueueService.Diagnostics.FailedSourceLine,
            FailedCommandText = executionQueueService.FailedPlannedCommand?.CommandText ?? executionQueueService.Diagnostics.FailedCommandText
        };

        var hasLoadedJob = jobSessionService.LoadedJob != null || executionQueueService.LoadedMotions.Count > 0;
        var jobWasInterrupted = jobSessionService.SessionState is CncJobLifecycleState.Interrupted or CncJobLifecycleState.Failed;
        var jobWasStopped = jobSessionService.SessionState == CncJobLifecycleState.Stopped || executionQueueService.ExecutionState == CncExecutionState.Stopped;
        var canSafelyResume = status.IsConnected
                              && !status.IsAlarmed
                              && status.HasValidReference
                              && executionQueueService.ExecutionState == CncExecutionState.Paused
                              && jobSessionService.SessionState == CncJobLifecycleState.Paused;

        if (status.RuntimeState == CncRuntimeState.Recovering)
        {
            plan.State = CncRecoveryState.Recovering;
            plan.Severity = CncRecoverySeverity.Warning;
            plan.Summary = "Recovery action is currently running.";
            plan.RequiredNextAction = "Wait for the current recovery step to finish.";
            return plan;
        }

        if (!status.IsConnected)
        {
            plan.State = CncRecoveryState.DisconnectDetected;
            plan.Reason = CncRecoveryReason.Disconnect;
            plan.Severity = hasLoadedJob || jobWasInterrupted ? CncRecoverySeverity.Critical : CncRecoverySeverity.Warning;
            plan.Summary = hasLoadedJob || jobWasInterrupted
                ? "Disconnect happened during or around a job. Machine position and reference can no longer be trusted."
                : "Machine disconnected. Reconnect and refresh status before continuing.";
            plan.RequiredNextAction = "Reconnect the controller.";
            plan.AllowedActions.Add(CncRecoveryAction.Reconnect);
            if (hasLoadedJob)
            {
                plan.AllowedActions.Add(CncRecoveryAction.ClearJob);
                plan.AllowedActions.Add(CncRecoveryAction.AbortJob);
                plan.BlockedActions.Add(CncRecoveryAction.ResumeJob);
                plan.BlockedActions.Add(CncRecoveryAction.RestartJob);
            }

            plan.RequiresRehome = status.ControllerMode == CncControllerMode.RealHardware;
            plan.CanTrustWorkOffset = false;
            plan.PreferRestartOverResume = true;
            plan.ResumeBlockedReason = "Resume is blocked after disconnect until reconnect, reference validation, and homing are completed.";
            return plan;
        }

        if (status.IsAlarmed || executionQueueService.ExecutionState == CncExecutionState.Alarmed)
        {
            plan.State = CncRecoveryState.AlarmDetected;
            plan.Reason = CncRecoveryReason.Alarm;
            plan.Severity = CncRecoverySeverity.Critical;
            plan.Summary = BuildAlarmSummary(plan, status);
            plan.RequiredNextAction = "Clear the alarm or unlock the controller, then re-home before running again.";
            plan.AllowedActions.Add(CncRecoveryAction.RefreshStatus);
            plan.AllowedActions.Add(CncRecoveryAction.ClearAlarm);
            plan.AllowedActions.Add(CncRecoveryAction.UnlockController);
            plan.AllowedActions.Add(CncRecoveryAction.RehomeMachine);
            if (hasLoadedJob)
            {
                plan.AllowedActions.Add(CncRecoveryAction.ClearJob);
                plan.AllowedActions.Add(CncRecoveryAction.RestartJob);
                plan.AllowedActions.Add(CncRecoveryAction.AbortJob);
            }

            plan.RequiresRehome = true;
            plan.CanTrustWorkOffset = false;
            plan.PreferRestartOverResume = true;
            plan.BlockedActions.Add(CncRecoveryAction.ResumeJob);
            plan.ResumeBlockedReason = "Resume is blocked while an alarm is active.";
            return plan;
        }

        if (!status.HasValidReference)
        {
            plan.State = status.ReferenceState.ReferenceLostReason == CncReferenceLostReason.Reset
                ? CncRecoveryState.ControllerReset
                : CncRecoveryState.ReferenceLost;
            plan.Reason = status.ReferenceState.ReferenceLostReason == CncReferenceLostReason.Reset
                ? CncRecoveryReason.ControllerReset
                : CncRecoveryReason.ReferenceLost;
            plan.Severity = CncRecoverySeverity.Warning;
            plan.Summary = status.ReferenceWarningText ?? "Machine reference is not valid.";
            plan.RequiredNextAction = "Re-home the machine before jogging, framing, or running.";
            plan.AllowedActions.Add(CncRecoveryAction.RefreshStatus);
            if (status.IsLocked)
                plan.AllowedActions.Add(CncRecoveryAction.UnlockController);
            plan.AllowedActions.Add(CncRecoveryAction.RehomeMachine);
            if (hasLoadedJob)
            {
                plan.AllowedActions.Add(CncRecoveryAction.RestartJob);
                plan.AllowedActions.Add(CncRecoveryAction.ClearJob);
            }

            plan.RequiresRehome = true;
            plan.CanTrustWorkOffset = false;
            plan.AllowedActions.Add(CncRecoveryAction.ResetWorkOffset);
            plan.PreferRestartOverResume = true;
            plan.BlockedActions.Add(CncRecoveryAction.ResumeJob);
            plan.ResumeBlockedReason = "Resume is blocked until machine reference is re-established.";
            return plan;
        }

        if (status.IsLocked)
        {
            plan.State = CncRecoveryState.RequiresUnlock;
            plan.Reason = CncRecoveryReason.Locked;
            plan.Severity = CncRecoverySeverity.Warning;
            plan.Summary = "Machine is locked. Motion commands are blocked until the controller is unlocked.";
            plan.RequiredNextAction = "Unlock the controller. Re-home if reference becomes invalid.";
            plan.AllowedActions.Add(CncRecoveryAction.UnlockController);
            plan.AllowedActions.Add(CncRecoveryAction.RefreshStatus);
            return plan;
        }

        if (!status.IsHomed && status.ControllerMode == CncControllerMode.RealHardware)
        {
            plan.State = CncRecoveryState.RequiresRehome;
            plan.Reason = CncRecoveryReason.HomingRequired;
            plan.Severity = CncRecoverySeverity.Warning;
            plan.Summary = "Machine is connected but not homed.";
            plan.RequiredNextAction = "Home the machine before running a job.";
            plan.AllowedActions.Add(CncRecoveryAction.RehomeMachine);
            plan.AllowedActions.Add(CncRecoveryAction.RefreshStatus);
            plan.RequiresRehome = true;
            plan.CanTrustWorkOffset = false;
            plan.AllowedActions.Add(CncRecoveryAction.ResetWorkOffset);
            return plan;
        }

        if (canSafelyResume)
        {
            var trueFeedHold = status.FirmwareIdentity.Capabilities.SupportsFeedHold;
            plan.State = CncRecoveryState.ReadyToRecover;
            plan.Reason = CncRecoveryReason.None;
            plan.Severity = CncRecoverySeverity.Info;
            plan.Summary = trueFeedHold
                ? "Execution is paused safely. You can resume the job or abort it."
                : "Execution is app-paused safely. The in-flight command already finished and the next unsent command can be resumed.";
            plan.RequiredNextAction = trueFeedHold
                ? "Resume or abort the paused job."
                : "Resume the next unsent command or abort the job.";
            plan.AllowedActions.Add(CncRecoveryAction.ResumeJob);
            plan.AllowedActions.Add(CncRecoveryAction.AbortJob);
            if (hasLoadedJob)
                plan.AllowedActions.Add(CncRecoveryAction.RestartJob);
            plan.ResumeAllowed = true;
            plan.CanTrustWorkOffset = true;
            return plan;
        }

        if (jobWasInterrupted || jobWasStopped || executionQueueService.ExecutionState is CncExecutionState.Failed or CncExecutionState.Error)
        {
            plan.State = jobWasStopped ? CncRecoveryState.StopRequested : CncRecoveryState.JobInterrupted;
            plan.Reason = jobWasStopped ? CncRecoveryReason.StopRequested : CncRecoveryReason.JobInterrupted;
            plan.Severity = CncRecoverySeverity.Warning;
            plan.Summary = jobWasStopped
                ? "The job was stopped before completion. Verify machine state before restarting."
                : "The active job was interrupted. Safe restart is preferred over resume.";
            plan.RequiredNextAction = status.HasValidReference
                ? "Review machine status, then restart the job or clear it."
                : "Re-home the machine before attempting a restart.";
            plan.AllowedActions.Add(CncRecoveryAction.RefreshStatus);
            if (hasLoadedJob)
            {
                plan.AllowedActions.Add(CncRecoveryAction.RestartJob);
                plan.AllowedActions.Add(CncRecoveryAction.ClearJob);
                plan.AllowedActions.Add(CncRecoveryAction.AbortJob);
            }

            plan.ResumeAllowed = false;
            plan.PreferRestartOverResume = true;
            plan.CanTrustWorkOffset = status.HasValidReference;
            plan.RequiresRehome = !status.HasValidReference;
            plan.BlockedActions.Add(CncRecoveryAction.ResumeJob);
            plan.ResumeBlockedReason = "Safe resume from the last line is not available yet. Restart is safer.";
            return plan;
        }

        return plan;
    }

    private static string BuildAlarmSummary(CncRecoveryPlan plan, CncRuntimeStatus status)
    {
        if (plan.FailedSourceLine.HasValue && !string.IsNullOrWhiteSpace(plan.FailedCommandText))
            return $"Alarm detected on line {plan.FailedSourceLine.Value}: {plan.FailedCommandText}";

        if (plan.FailedSourceLine.HasValue)
            return $"Alarm detected on line {plan.FailedSourceLine.Value}.";

        return status.LastAlarmMessage == "No active alarm."
            ? "Controller alarm detected."
            : status.LastAlarmMessage;
    }
}
