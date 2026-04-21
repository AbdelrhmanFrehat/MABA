using Maba.Domain.MachineCatalog.Enums;

namespace Maba.Domain.MachineCatalog.Sections;

// ── RuntimeBinding ────────────────────────────────────────────────────────

public class RuntimeBindingSection
{
    public DriverType DefaultDriverType { get; set; } = DriverType.Unknown;
    public List<DriverType> SupportedDriverTypes { get; set; } = new();
    public FirmwareProtocol FirmwareProtocol { get; set; } = FirmwareProtocol.Unknown;
    public List<SetupMode> SupportedSetupModes { get; set; } = new();
    public VisualizationType VisualizationType { get; set; } = VisualizationType.Generic2D;
    public KinematicsType KinematicsType { get; set; } = KinematicsType.Unknown;

    /// <summary>
    /// Naming convention: {category}-{variant}-{version}  e.g. "cnc-standard-v1"
    /// App resolves to internal UI module; unknown values fall back to "generic-v1".
    /// </summary>
    public string RuntimeUiVariant { get; set; } = "generic-v1";
}

// ── AxisConfig ────────────────────────────────────────────────────────────

public class AxisConfigSection
{
    public int AxisCount { get; set; }
    public List<AxisId> SupportedAxes { get; set; } = new();

    // Dictionary key is AxisId string (X/Y/Z/A/B/C) for JSON compatibility
    public Dictionary<string, AxisRole> AxisRoles { get; set; } = new();
    public Dictionary<string, Direction> AxisDirections { get; set; } = new();
    public Dictionary<string, bool> HomingSupport { get; set; } = new();

    public HomeOriginConvention HomeOriginConvention { get; set; } = HomeOriginConvention.FrontLeft;
    public bool WorkCoordinateSupport { get; set; }
    public bool MachineCoordinateSupport { get; set; }
    public bool RelativeMoveSupport { get; set; }
    public bool AbsoluteMoveSupport { get; set; }
}

// ── Workspace ─────────────────────────────────────────────────────────────

public class WorkspaceSection
{
    public Dictionary<string, double> MaxTravelMm { get; set; } = new();
    public Dictionary<string, double>? MinTravelMm { get; set; }
    public WorkAreaDimensions WorkAreaMm { get; set; } = new();
    public MachineDimensions? MachineDimensionsMm { get; set; }
    public double? SafeZHeightMm { get; set; }
    public Dictionary<string, double>? ParkPositionMm { get; set; }
}

public class WorkAreaDimensions
{
    public double Width { get; set; }
    public double Depth { get; set; }
    public double Height { get; set; }
}

public class MachineDimensions
{
    public double Width { get; set; }
    public double Depth { get; set; }
    public double Height { get; set; }
}

// ── MotionDefaults ────────────────────────────────────────────────────────

public class MotionDefaultsSection
{
    public Dictionary<string, double> StepsPerMm { get; set; } = new();
    public Dictionary<string, double> MaxFeedMmMin { get; set; } = new();
    public Dictionary<string, double>? MaxAccelMmSec2 { get; set; }
    public List<JogPreset> JogPresets { get; set; } = new();
}

public class JogPreset
{
    public string Label { get; set; } = string.Empty;
    public double FeedMmMin { get; set; }
    public double DistanceMm { get; set; }
}

// ── ConnectionDefaults ────────────────────────────────────────────────────

public class ConnectionDefaultsSection
{
    public int DefaultBaudRate { get; set; }
    public List<int> SupportedBaudRates { get; set; } = new();
    public List<ConnectionType> SupportedConnectionTypes { get; set; } = new();
    public bool RequiresHandshake { get; set; }
    public string CommandTerminator { get; set; } = "\n";
    public string? ResponseAckPattern { get; set; }
    public string? ProtocolNotes { get; set; }
}

// ── Capabilities ──────────────────────────────────────────────────────────

public class CapabilitiesSection
{
    public MotionCapabilities Motion { get; set; } = new();
    public ExecutionCapabilities Execution { get; set; } = new();
    public ProtocolCapabilities Protocol { get; set; } = new();
    public VisualizationCapabilities Visualization { get; set; } = new();
    public FileHandlingCapabilities FileHandling { get; set; } = new();
}

public class MotionCapabilities
{
    public bool Homing { get; set; }
    public bool ZHoming { get; set; }
    public bool CombinedXYHoming { get; set; }
    public bool RelativeMoves { get; set; }
    public bool AbsoluteMoves { get; set; }
    public bool Pause { get; set; }
    public bool Resume { get; set; }
    public bool Stop { get; set; }
    public bool Park { get; set; }
    public bool CenterMove { get; set; }
    public bool WorkOffset { get; set; }
    public bool JogContinuous { get; set; }
    public bool JogStep { get; set; }
}

public class ExecutionCapabilities
{
    public bool RealExecution { get; set; }
    public bool Simulation { get; set; }
    public bool PreviewPlayback { get; set; }
    public bool DryRun { get; set; }
    public bool FileRun { get; set; }
    public bool Frame { get; set; }
    public bool BoundingBoxPreview { get; set; }
    public bool LiveReportedPosition { get; set; }
    public bool EstimatedPositionOnly { get; set; }
    public bool ToolpathPreview { get; set; }
    public bool ProgressTracking { get; set; }
}

public class ProtocolCapabilities
{
    public bool Handshake { get; set; }
    public bool Acknowledgements { get; set; }
    public bool AlarmReporting { get; set; }
    public bool AlarmReset { get; set; }
    public bool StatusQuery { get; set; }
    public bool PositionQuery { get; set; }
    public bool MotorEnable { get; set; }
    public bool MotorDisable { get; set; }
    public bool FeedHold { get; set; }
    public bool SoftReset { get; set; }
}

public class VisualizationCapabilities
{
    public bool MachineVisualization { get; set; }
    public bool TopView2D { get; set; }
    public bool Perspective3D { get; set; }
    public bool KinematicsAnimation { get; set; }
    public bool RealTimePositionDisplay { get; set; }
}

public class FileHandlingCapabilities
{
    public bool LocalFileRun { get; set; }
    public bool StreamingExecution { get; set; }
    public bool GcodeValidation { get; set; }
    public bool MultipleFileFormats { get; set; }
}

// ── FileSupport ───────────────────────────────────────────────────────────

public class FileSupportSection
{
    public List<string> SupportedInputFileTypes { get; set; } = new();
    public GcodeDialect GcodeDialect { get; set; } = GcodeDialect.Generic;
    public List<OperationType> SupportedOperationTypes { get; set; } = new();
}

// ── Visualization ─────────────────────────────────────────────────────────

public class VisualizationSection
{
    public VisualizationType VisualizationType { get; set; } = VisualizationType.Generic2D;
    public KinematicsType KinematicsType { get; set; } = KinematicsType.Unknown;
    public CoordinateMode CoordinatePresentationMode { get; set; } = CoordinateMode.BottomLeft;
    public MachineShapeHint MachineShapeHint { get; set; } = MachineShapeHint.Rectangular;
    public ViewMode DefaultViewMode { get; set; } = ViewMode.Top2D;
}

// ── ProfileRules ──────────────────────────────────────────────────────────

public class ProfileRulesSection
{
    public List<OverrideField> AllowedOverrides { get; set; } = new();
    public List<OverrideConstraint> OverrideConstraints { get; set; } = new();
    public BuiltInProfileRules BuiltInProfileRules { get; set; } = new();
    public UserProfileRules UserProfileRules { get; set; } = new();
}

public class OverrideConstraint
{
    public OverrideField Field { get; set; }
    public ConstraintType ConstraintType { get; set; }
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public List<string>? AllowedValues { get; set; }
    public double? MaxDeltaPercent { get; set; }
}

public class BuiltInProfileRules
{
    public bool IsEditable { get; set; } = false;
    public bool IsDeletable { get; set; } = false;
    public bool IsDuplicatable { get; set; } = true;
    public string DuplicateProducesType { get; set; } = "User";
}

public class UserProfileRules
{
    public bool IsEditable { get; set; } = true;
    public bool IsDeletable { get; set; } = true;
    public bool IsDuplicatable { get; set; } = true;
    public string DuplicateProducesType { get; set; } = "User";
    public int? MaxUserProfiles { get; set; }
}
