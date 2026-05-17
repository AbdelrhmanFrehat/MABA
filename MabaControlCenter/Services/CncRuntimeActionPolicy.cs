using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class CncRuntimeActionPolicyService : ICncRuntimeActionPolicy
{
    private readonly ICncControllerStateMachine _stateMachine;

    public CncRuntimeActionPolicyService(ICncControllerStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }

    public CncRuntimeActionPolicy Build(CncRuntimeStatus status)
    {
        var policy = new CncRuntimeActionPolicy();
        foreach (var action in Enum.GetValues<CncRuntimeAction>())
            policy.Set(BuildDescriptor(status, action));

        return policy;
    }

    private CncRuntimeActionDescriptor BuildDescriptor(CncRuntimeStatus status, CncRuntimeAction action)
    {
        var descriptor = new CncRuntimeActionDescriptor
        {
            Action = action
        };

        if (!_stateMachine.CanExecute(status, action, out var stateReason))
        {
            descriptor.IsAllowed = false;
            descriptor.Reason = stateReason;
            descriptor.Severity = CncRuntimeActionSeverity.Warning;
            return descriptor;
        }

        if (action is CncRuntimeAction.Run or CncRuntimeAction.Frame && status.BlockingReasons.Count > 0)
        {
            descriptor.IsAllowed = false;
            descriptor.Reason = status.BlockingReason;
            descriptor.Severity = CncRuntimeActionSeverity.Critical;
            descriptor.RequiredRecoveryAction = status.HasValidReference ? null : CncRecoveryAction.RehomeMachine;
            return descriptor;
        }

        if (status.ControllerMode == CncControllerMode.RealHardware)
        {
            if (RequiresFreshVerifiedControllerState(action)
                && status.ControllerStatusConfidence is CncControllerStatusConfidence.Unknown or CncControllerStatusConfidence.Stale)
            {
                descriptor.IsAllowed = false;
                descriptor.Reason = status.ControllerStatusWarningText ?? "Controller status is not verified yet.";
                descriptor.Severity = CncRuntimeActionSeverity.Critical;
                descriptor.DependsOnVerifiedControllerState = true;
                descriptor.RequiredRecoveryAction = CncRecoveryAction.RefreshStatus;
                return descriptor;
            }

            if (status.FirmwareCompatibility.Status == CncFirmwareCompatibilityStatus.Incompatible
                && IsFirmwareDependentAction(action))
            {
                descriptor.IsAllowed = false;
                descriptor.Reason = status.FirmwareCompatibility.Errors.FirstOrDefault()
                                    ?? "Connected firmware is incompatible with the selected machine profile.";
                descriptor.Severity = CncRuntimeActionSeverity.Critical;
                descriptor.DependsOnVerifiedFirmwareCapability = true;
                return descriptor;
            }

            if (IsFirmwareDependentAction(action))
            {
                var capabilityState = ResolveCapabilityState(status, action);
                descriptor.DependsOnVerifiedFirmwareCapability = capabilityState is not RuntimeCapabilityState.Unsupported and not RuntimeCapabilityState.None;

                switch (capabilityState)
                {
                    case RuntimeCapabilityState.Unknown:
                        descriptor.IsAllowed = false;
                        descriptor.Reason = "Firmware capability is unknown. Refresh firmware info or reconnect before using this action.";
                        descriptor.Severity = CncRuntimeActionSeverity.Critical;
                        descriptor.RequiredRecoveryAction = CncRecoveryAction.RefreshStatus;
                        return descriptor;
                    case RuntimeCapabilityState.Unsupported:
                        descriptor.IsAllowed = false;
                        descriptor.Reason = $"Connected firmware does not support '{action}'.";
                        descriptor.Severity = CncRuntimeActionSeverity.Critical;
                        return descriptor;
                    case RuntimeCapabilityState.Inferred:
                        descriptor.IsAllowed = true;
                        descriptor.Reason = "Firmware capability is inferred, not verified.";
                        descriptor.Severity = CncRuntimeActionSeverity.Warning;
                        return descriptor;
                }
            }
        }

        descriptor.IsAllowed = true;
        descriptor.Severity = CncRuntimeActionSeverity.Info;
        return descriptor;
    }

    private static bool RequiresFreshVerifiedControllerState(CncRuntimeAction action)
    {
        return action is CncRuntimeAction.SetWorkZero
            or CncRuntimeAction.GoToCenter
            or CncRuntimeAction.Frame
            or CncRuntimeAction.Run
            or CncRuntimeAction.Pause
            or CncRuntimeAction.Resume;
    }

    private static bool IsFirmwareDependentAction(CncRuntimeAction action)
    {
        return action is CncRuntimeAction.Unlock
            or CncRuntimeAction.DisableMotors
            or CncRuntimeAction.Home
            or CncRuntimeAction.Jog
            or CncRuntimeAction.Frame
            or CncRuntimeAction.Run
            or CncRuntimeAction.Pause
            or CncRuntimeAction.Resume
            or CncRuntimeAction.ResetAlarm
            or CncRuntimeAction.SetWorkZero
            or CncRuntimeAction.ClearWorkZero
            or CncRuntimeAction.GoToCenter;
    }

    private static RuntimeCapabilityState ResolveCapabilityState(CncRuntimeStatus status, CncRuntimeAction action)
    {
        if (status.ControllerMode == CncControllerMode.Simulation)
            return RuntimeCapabilityState.Verified;

        if (!status.FirmwareIdentity.IsKnown)
            return action is CncRuntimeAction.Connect or CncRuntimeAction.Disconnect
                ? RuntimeCapabilityState.None
                : RuntimeCapabilityState.Unknown;

        var supports = action switch
        {
            CncRuntimeAction.Unlock => status.FirmwareIdentity.Capabilities.SupportsUnlock || status.FirmwareIdentity.Capabilities.SupportsMotorEnable,
            CncRuntimeAction.DisableMotors => status.FirmwareIdentity.Capabilities.SupportsMotorDisable,
            CncRuntimeAction.Home => status.FirmwareIdentity.Capabilities.SupportsHoming,
            CncRuntimeAction.Jog => status.FirmwareIdentity.Capabilities.SupportsJog,
            CncRuntimeAction.Frame => status.FirmwareIdentity.Capabilities.SupportsG0G1,
            CncRuntimeAction.Run => status.FirmwareIdentity.Capabilities.SupportsG0G1,
            CncRuntimeAction.Pause => status.FirmwareIdentity.Capabilities.SupportsFeedHold || status.FirmwareIdentity.Capabilities.SupportsSoftwareStop,
            CncRuntimeAction.Resume => status.FirmwareIdentity.Capabilities.SupportsFeedHold || status.FirmwareIdentity.Capabilities.SupportsG0G1,
            CncRuntimeAction.Stop => status.FirmwareIdentity.Capabilities.SupportsSoftwareStop,
            CncRuntimeAction.ResetAlarm => status.FirmwareIdentity.Capabilities.SupportsUnlock,
            CncRuntimeAction.RefreshStatus => status.FirmwareIdentity.Capabilities.SupportsStatusQuery,
            CncRuntimeAction.SetWorkZero => true,
            CncRuntimeAction.ClearWorkZero => true,
            CncRuntimeAction.GoToCenter => status.FirmwareIdentity.Capabilities.SupportsG0G1,
            _ => true
        };

        if (!supports)
            return RuntimeCapabilityState.Unsupported;

        return status.FirmwareIdentity.Confidence switch
        {
            CncCapabilityConfidence.Verified => RuntimeCapabilityState.Verified,
            CncCapabilityConfidence.Inferred => RuntimeCapabilityState.Inferred,
            _ => RuntimeCapabilityState.Unknown
        };
    }

    private enum RuntimeCapabilityState
    {
        None,
        Unknown,
        Inferred,
        Verified,
        Unsupported
    }
}
