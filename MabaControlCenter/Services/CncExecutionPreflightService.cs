using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class CncExecutionPreflightService : ICncExecutionPreflightService
{
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
            if (!string.IsNullOrWhiteSpace(transformed.BoundsMessage))
            {
                result.Failures.Add(transformed.BoundsMessage);
                break;
            }
        }

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
}
