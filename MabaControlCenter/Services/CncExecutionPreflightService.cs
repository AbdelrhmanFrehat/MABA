using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class CncExecutionPreflightService : ICncExecutionPreflightService
{
    private static readonly Version MinimumManualZSafeFirmwareVersion = new(2, 1, 1);
    private readonly ICncCoordinateTransformService _coordinateTransformService;

    public CncExecutionPreflightService(ICncCoordinateTransformService coordinateTransformService)
    {
        _coordinateTransformService = coordinateTransformService;
    }

    public CncExecutionPreflightResult Evaluate(CncExecutionPreflightRequest request, ICncControllerService controllerService)
    {
        var result = new CncExecutionPreflightResult();
        var action = request.Intent == CncExecutionIntent.Run ? CncRuntimeAction.Run : CncRuntimeAction.Frame;
        var descriptor = request.RuntimeStatus.GetActionDescriptor(action);
        var safeTravelZ = request.MachineConfig.SafeTravelZMm > 0m ? request.MachineConfig.SafeTravelZMm : 5m;
        result.TrustState = BuildTrustState(request);
        var deferredNegativeZBounds = false;
        string? detectedBoundsFailure = null;

        if (!descriptor.IsAllowed)
            result.Failures.Add(descriptor.Reason ?? $"{action} is blocked right now.");

        if (request.LoadedProgram == null)
            result.Failures.Add("No CNC job is currently loaded.");

        if (request.MotionCommands.Count == 0)
            result.Failures.Add("No executable CNC motions are currently available.");

        if (request.ParserErrorCount > 0 || request.MotionCommands.Any(motion => !motion.IsValid))
            result.Failures.Add("Resolve parser or bounds errors before starting the job.");

        if (request.LoadedProgram?.UnsupportedCommandCount > 0)
            result.Failures.Add("Unsupported G-code commands or planes are blocking execution.");

        if (request.RuntimeStatus.FirmwareCompatibility.Status == CncFirmwareCompatibilityStatus.Incompatible)
            result.Failures.Add(request.RuntimeStatus.FirmwareCompatibility.Errors.FirstOrDefault()
                                ?? "Connected firmware is incompatible with the selected machine profile.");

        if (request.RuntimeStatus.ControllerMode == CncControllerMode.RealHardware)
        {
            if (request.RuntimeStatus.ControllerStatusConfidence is CncControllerStatusConfidence.Unknown or CncControllerStatusConfidence.Stale)
                result.Failures.Add(request.RuntimeStatus.ControllerStatusWarningText ?? "Controller state is not verified.");

            if (!request.RuntimeStatus.HasValidReference)
                result.Failures.Add(request.RuntimeStatus.ReferenceWarningText ?? "Machine reference is unknown.");

            if (request.MachineConfig.SupportsZAxis
                && request.MachineConfig.RequireManualZZeroForCutting
                && RequiresZReference(request.InterpretedCommands, safeTravelZ)
                && !request.RuntimeStatus.ReferenceState.ZReferenceValid)
            {
                result.Failures.Add("Z is not calibrated. Manually jog the tool to the material surface and press Set Z Zero before running.");
            }

            var firmwareGateFailure = GetManualZFirmwareGateFailure(request);
            if (!string.IsNullOrWhiteSpace(firmwareGateFailure))
                result.Failures.Add(firmwareGateFailure);
        }

        if (!request.MachineConfig.SupportsZAxis && RequiresZMotion(request.InterpretedCommands))
            result.Failures.Add("This job requires Z motion or safe Z travel, but the active machine profile does not support Z.");

        if (request.InterpretedCommands.Any(command => command.IsSpindleChange)
            && request.RuntimeStatus.FirmwareIdentity.IsKnown
            && !request.RuntimeStatus.FirmwareIdentity.Capabilities.SupportsSpindleOnOff)
        {
            result.Failures.Add("Loaded job requests spindle commands, but the connected firmware does not support M3/M5.");
        }

        foreach (var command in request.InterpretedCommands.Where(command => command.HasMotion && command.MotionType != null))
        {
            var state = controllerService.CoordinateState.Clone();
            state.JobPlacementOffset = new CncJobPlacementOffset
            {
                X = command.ModalStateAfterLine.Coordinates.PlacementOffsetX,
                Y = command.ModalStateAfterLine.Coordinates.PlacementOffsetY,
                Z = command.ModalStateAfterLine.Coordinates.PlacementOffsetZ
            };

            var transformed = _coordinateTransformService.FlattenForFirmware(command.EndX, command.EndY, command.EndZ, state);
            transformed = _coordinateTransformService.Validate(transformed, request.MachineBounds, request.MachineConfig);
            if (CanDeferNegativeZBounds(request, command, transformed.BoundsMessage))
            {
                transformed.BoundsMessage = null;
                deferredNegativeZBounds = true;
            }
            if (!string.IsNullOrWhiteSpace(transformed.BoundsMessage))
            {
                detectedBoundsFailure = transformed.BoundsMessage;
                result.Failures.Add(transformed.BoundsMessage);
                break;
            }
        }

        result.TrustState.BoundsValid = string.IsNullOrWhiteSpace(detectedBoundsFailure) || deferredNegativeZBounds;
        ApplyWorkflowGuidance(request, result);

        return result;
    }

    private static bool RequiresZMotion(IReadOnlyList<GcodeInterpretedCommand> commands)
    {
        return commands.Any(command =>
            command.HasMotion
            && (command.StartZ != command.EndZ || command.StartZ != 0m || command.EndZ != 0m));
    }

    private static bool RequiresZReference(IReadOnlyList<GcodeInterpretedCommand> commands, decimal safeTravelZ)
    {
        return commands.Any(command =>
            command.HasMotion
            && (command.StartZ != command.EndZ || command.StartZ != 0m || command.EndZ != 0m));
    }

    private static bool CanDeferNegativeZBounds(CncExecutionPreflightRequest request, GcodeInterpretedCommand command, string? boundsMessage)
    {
        if (string.IsNullOrWhiteSpace(boundsMessage))
            return false;

        if (!boundsMessage.StartsWith("Bounds violation: Z", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!request.MachineConfig.SupportsZAxis || !request.MachineConfig.RequireManualZZeroForCutting)
            return false;

        if (request.RuntimeStatus.ReferenceState.ZReferenceValid)
            return false;

        return command.EndZ < 0m || command.StartZ < 0m;
    }

    private static CncWorkflowTrustState BuildTrustState(CncExecutionPreflightRequest request)
    {
        var runtime = request.RuntimeStatus;
        var hasZCuttingMotion = RequiresZReference(request.InterpretedCommands, request.MachineConfig.SafeTravelZMm > 0m ? request.MachineConfig.SafeTravelZMm : 5m);
        var jobLoaded = request.LoadedProgram != null || request.MotionCommands.Count > 0;
        var jobParsed = request.LoadedProgram != null
                        && request.ParserErrorCount == 0
                        && request.LoadedProgram.UnsupportedCommandCount == 0;
        var jobPlanned = request.MotionCommands.Count > 0 && request.MotionCommands.All(motion => motion.IsValid);

        return new CncWorkflowTrustState
        {
            Connected = runtime.IsConnected,
            FirmwareReady = runtime.FirmwareCompatibility.Status != CncFirmwareCompatibilityStatus.Incompatible
                            && runtime.ControllerStatusConfidence != CncControllerStatusConfidence.Unknown
                            && runtime.ControllerStatusConfidence != CncControllerStatusConfidence.Stale,
            AlarmActive = runtime.IsAlarmed,
            XYReferenced = runtime.ReferenceState.XyReferenceValid,
            ZWorkZeroTrusted = !request.MachineConfig.SupportsZAxis || !request.MachineConfig.RequireManualZZeroForCutting || runtime.ReferenceState.ZReferenceValid,
            JobLoaded = jobLoaded,
            JobParsed = jobParsed,
            JobPlanned = jobPlanned,
            PlacementValid = request.PlacementValid,
            BoundsValid = true,
            HasZCuttingMotion = hasZCuttingMotion,
            IsRunning = runtime.RuntimeState == CncRuntimeState.Running,
            IsPaused = runtime.RuntimeState is CncRuntimeState.Paused or CncRuntimeState.FeedHold
        };
    }

    private static void ApplyWorkflowGuidance(CncExecutionPreflightRequest request, CncExecutionPreflightResult result)
    {
        var trust = result.TrustState;
        var runtime = request.RuntimeStatus;

        if (trust.IsRunning)
        {
            result.NextRequiredStep = CncWorkflowNextStep.Running;
            result.NextRequiredStepText = "Running: G-code is streaming to the machine.";
            return;
        }

        if (runtime.RuntimeState == CncRuntimeState.ProgramComplete)
        {
            result.NextRequiredStep = CncWorkflowNextStep.Complete;
            result.NextRequiredStepText = "Complete: Job finished successfully.";
            return;
        }

        if (runtime.IsAlarmed
            || runtime.ReferenceState.ReferenceLostReason is CncReferenceLostReason.Alarm or CncReferenceLostReason.Disconnect or CncReferenceLostReason.FaultRecoveryRequired)
        {
            result.NextRequiredStep = CncWorkflowNextStep.RecoveryRequired;
            result.NextRequiredStepText = runtime.ReferenceWarningText is { Length: > 0 }
                ? $"Recovery required: {runtime.ReferenceWarningText}"
                : "Recovery required before running.";
            return;
        }

        if (!trust.JobLoaded)
        {
            result.NextRequiredStep = request.HasPendingImageToolpathPreview
                ? CncWorkflowNextStep.CreateGcodeJob
                : CncWorkflowNextStep.ImportImageOrLoadGcode;
            result.NextRequiredStepText = request.HasPendingImageToolpathPreview
                ? "Next step: Create G-code job from image."
                : "Next step: Import an image or load G-code.";
            return;
        }

        if (!trust.Connected)
        {
            result.NextRequiredStep = CncWorkflowNextStep.ConnectMachine;
            result.NextRequiredStepText = "Next step: Connect machine.";
            return;
        }

        if (!trust.XYReferenced)
        {
            result.NextRequiredStep = CncWorkflowNextStep.HomeXY;
            result.NextRequiredStepText = "Next step: Home X/Y.";
            return;
        }

        if (trust.HasZCuttingMotion && !trust.ZWorkZeroTrusted)
        {
            result.NextRequiredStep = CncWorkflowNextStep.SetZZero;
            result.NextRequiredStepText = "Next step: Jog Z to material surface and press Set Z Zero.";
            return;
        }

        if (!trust.PlacementValid || !trust.BoundsValid)
        {
            result.NextRequiredStep = CncWorkflowNextStep.ResolvePlacement;
            result.NextRequiredStepText = "Next step: Resolve placement or bounds before running.";
            return;
        }

        if (!trust.JobParsed || !trust.JobPlanned || result.Failures.Count > 0)
        {
            result.NextRequiredStep = CncWorkflowNextStep.RunPreflight;
            result.NextRequiredStepText = $"Start blocked: {result.Summary ?? "Run preflight failed."}";
            return;
        }

        result.NextRequiredStep = CncWorkflowNextStep.ReadyToStart;
        result.NextRequiredStepText = "Ready: Preflight passed. You can start the job.";
    }

    private static string? GetManualZFirmwareGateFailure(CncExecutionPreflightRequest request)
    {
        if (!request.MachineConfig.SupportsZAxis
            || !request.MachineConfig.RequireManualZZeroForCutting
            || !RequiresZReference(request.InterpretedCommands, request.MachineConfig.SafeTravelZMm > 0m ? request.MachineConfig.SafeTravelZMm : 5m))
        {
            return null;
        }

        var identity = request.RuntimeStatus.FirmwareIdentity;
        if (!string.Equals(identity.ProtocolName, "MABA", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(request.RuntimeStatus.ProtocolVersion, "MabaProtocol", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!TryParseVersion(identity.FirmwareVersion, out var detectedVersion))
        {
            return $"Connected MABA firmware version is unknown. Upload bundled firmware v{ArduinoFirmwarePackage.FirmwareVersion} before running manual-Z jobs.";
        }

        if (detectedVersion < MinimumManualZSafeFirmwareVersion)
        {
            return $"Connected MABA firmware v{identity.FirmwareVersion} is too old for safe manual-Z jobs. Upload bundled firmware v{ArduinoFirmwarePackage.FirmwareVersion} before running.";
        }

        return null;
    }

    private static bool TryParseVersion(string? versionText, out Version version)
    {
        version = new Version(0, 0);
        if (string.IsNullOrWhiteSpace(versionText) || string.Equals(versionText, "Unknown", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!Version.TryParse(versionText.Trim(), out var parsedVersion) || parsedVersion == null)
            return false;

        version = parsedVersion;
        return true;
    }
}
