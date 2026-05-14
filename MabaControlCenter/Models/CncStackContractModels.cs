namespace MabaControlCenter.Models;

public enum CncRuntimeActionSeverity
{
    Info,
    Warning,
    Critical
}

public enum CncControllerStatusConfidence
{
    Unknown,
    VerifiedFresh,
    LastKnown,
    Stale
}

public enum CncSimulationMode
{
    NormalSimulation,
    StrictSimulation
}

public class CncRuntimeActionDescriptor
{
    public CncRuntimeAction Action { get; set; }
    public bool IsAllowed { get; set; }
    public string? Reason { get; set; }
    public CncRuntimeActionSeverity Severity { get; set; } = CncRuntimeActionSeverity.Info;
    public CncRecoveryAction? RequiredRecoveryAction { get; set; }
    public bool DependsOnVerifiedFirmwareCapability { get; set; }
    public bool DependsOnVerifiedControllerState { get; set; }
}

public class CncRuntimeActionPolicy
{
    private readonly Dictionary<CncRuntimeAction, CncRuntimeActionDescriptor> _descriptors = new();

    public IReadOnlyDictionary<CncRuntimeAction, CncRuntimeActionDescriptor> Descriptors => _descriptors;

    public CncRuntimeActionDescriptor this[CncRuntimeAction action]
        => _descriptors.TryGetValue(action, out var descriptor)
            ? descriptor
            : new CncRuntimeActionDescriptor
            {
                Action = action,
                IsAllowed = false,
                Reason = "No runtime action policy is available."
            };

    public void Set(CncRuntimeActionDescriptor descriptor)
    {
        _descriptors[descriptor.Action] = descriptor;
    }
}

public enum CncExecutionIntent
{
    Run,
    Frame
}

public class CncExecutionPreflightRequest
{
    public CncExecutionIntent Intent { get; set; }
    public CncRuntimeStatus RuntimeStatus { get; set; } = new();
    public CncMachineConfig MachineConfig { get; set; } = new();
    public CncMachineBounds MachineBounds { get; set; } = new();
    public GcodeParseResult? LoadedProgram { get; set; }
    public IReadOnlyList<GcodeMotionCommand> MotionCommands { get; set; } = Array.Empty<GcodeMotionCommand>();
    public IReadOnlyList<GcodeInterpretedCommand> InterpretedCommands { get; set; } = Array.Empty<GcodeInterpretedCommand>();
    public int ParserErrorCount { get; set; }
}

public class CncExecutionPreflightResult
{
    public List<string> Failures { get; } = new();
    public List<string> Warnings { get; } = new();
    public bool IsAllowed => Failures.Count == 0;
    public string? Summary => Failures.FirstOrDefault() ?? Warnings.FirstOrDefault();
}

public class CncManagerOperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Error { get; set; }
    public CncExecutionPreflightResult? Preflight { get; set; }
}
