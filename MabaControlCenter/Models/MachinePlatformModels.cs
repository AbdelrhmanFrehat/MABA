namespace MabaControlCenter.Models;

public enum DriverType { ArduinoSerial, GRBL, Marlin, RepRap, MabaCustom, Simulated, NetworkMaba, Unknown }
public enum FirmwareProtocol { GRBL_1_1, GRBL_0_9, Marlin_2, Marlin_1, RepRap, MabaProtocol, Custom, Unknown }
public enum KinematicsType { MovingGantryXY, FixedGantryMovingBed, CoreXY, Delta, LaserFlatbed, CartesianPrinter, Scara, Unknown }
public enum VisualizationType { CncTopDown2D, LaserFlatbed2D, Printer3DCartesian, Printer3DDelta, Generic2D, Generic3D }
public enum SetupMode { RealOnly, SimulationOnly, RealAndSimulation }
public enum ConnectionType { Serial, USB, Network, Bluetooth, Simulated }
public enum MachineHomeOriginConvention { FrontLeft, FrontRight, BackLeft, BackRight, Center, Custom }
public enum OperationType { Milling, Engraving, Drilling, LaserCutting, LaserEngraving, FDMPrint, ResinPrint, Plotting }
public enum GcodeDialect { GRBL, Marlin, RepRap, MabaCustom, Generic }
public enum ViewMode { Top2D, Perspective3D, Side2D, Isometric }
public enum CoordinateMode { TopLeft, BottomLeft, Center }
public enum MachineShapeHint { Rectangular, Delta, Polar, Articulated }
public enum AxisId { X, Y, Z, A, B, C }
public enum AxisRole { Primary, Secondary, Vertical, Rotational, Extruder, Generic }
public enum Direction { Normal, Inverted }
public enum OverrideField { DriverType, BaudRate, Port, StepsPerMm, MaxFeed, MaxAccel, JogPresets, SafeZ, ParkPosition, WorkOffset, Visualization, Notes }
public enum ConstraintType { Range, AllowedValues, MaxDeltaPercent, FreeText, None }
public enum RuntimeProfileType { System, User }
public enum DefinitionCompatibilityState { Current, DefinitionUpdated, DefinitionIncompatible, DefinitionMissing }

public class MachineCategory
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DisplayNameEn { get; set; } = string.Empty;
    public string DisplayNameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? IconKey { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class MachineFamily
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryDisplayNameEn { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DisplayNameEn { get; set; } = string.Empty;
    public string DisplayNameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string Manufacturer { get; set; } = "MABA";
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class MachineDefinitionSummary
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string? CategoryDisplayNameEn { get; set; }
    public Guid FamilyId { get; set; }
    public string? FamilyDisplayNameEn { get; set; }
    public string DisplayNameEn { get; set; } = string.Empty;
    public string DisplayNameAr { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public bool IsDeprecated { get; set; }
    public string? DeprecationNote { get; set; }
    public int SortOrder { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public List<string> Tags { get; set; } = new();
    public string DefaultDriverType { get; set; } = string.Empty;
    public List<string> SupportedSetupModes { get; set; } = new();
    public string RuntimeUiVariant { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class MachineDefinition
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? RevisionNote { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryDisplayNameEn { get; set; }
    public Guid FamilyId { get; set; }
    public string? FamilyDisplayNameEn { get; set; }
    public string DisplayNameEn { get; set; } = string.Empty;
    public string DisplayNameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public bool IsDeprecated { get; set; }
    public string? DeprecationNote { get; set; }
    public int SortOrder { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public string? InternalNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public RuntimeBindingSection RuntimeBinding { get; set; } = new();
    public AxisConfigSection AxisConfig { get; set; } = new();
    public WorkspaceSection Workspace { get; set; } = new();
    public MotionDefaultsSection MotionDefaults { get; set; } = new();
    public ConnectionDefaultsSection ConnectionDefaults { get; set; } = new();
    public CapabilitiesSection Capabilities { get; set; } = new();
    public FileSupportSection FileSupport { get; set; } = new();
    public VisualizationSection Visualization { get; set; } = new();
    public ProfileRulesSection ProfileRules { get; set; } = new();
}

public class RuntimeBindingSection
{
    public DriverType DefaultDriverType { get; set; } = DriverType.Unknown;
    public List<DriverType> SupportedDriverTypes { get; set; } = new();
    public FirmwareProtocol FirmwareProtocol { get; set; } = FirmwareProtocol.Unknown;
    public List<SetupMode> SupportedSetupModes { get; set; } = new();
    public VisualizationType VisualizationType { get; set; } = VisualizationType.Generic2D;
    public KinematicsType KinematicsType { get; set; } = KinematicsType.Unknown;
    public string RuntimeUiVariant { get; set; } = "generic-v1";
}

public class AxisConfigSection
{
    public int AxisCount { get; set; }
    public List<AxisId> SupportedAxes { get; set; } = new();
    public Dictionary<string, AxisRole> AxisRoles { get; set; } = new();
    public Dictionary<string, Direction> AxisDirections { get; set; } = new();
    public Dictionary<string, bool> HomingSupport { get; set; } = new();
    public MachineHomeOriginConvention HomeOriginConvention { get; set; } = MachineHomeOriginConvention.FrontLeft;
    public bool WorkCoordinateSupport { get; set; }
    public bool MachineCoordinateSupport { get; set; }
    public bool RelativeMoveSupport { get; set; }
    public bool AbsoluteMoveSupport { get; set; }
}

public class WorkspaceSection
{
    public Dictionary<string, double> MaxTravelMm { get; set; } = new();
    public Dictionary<string, double>? MinTravelMm { get; set; }
    public WorkAreaDimensions WorkAreaMm { get; set; } = new();
    public MachineDimensions? MachineDimensionsMm { get; set; }
    public double? SafeZHeightMm { get; set; }
    public Dictionary<string, double>? ParkPositionMm { get; set; }
}

public class WorkAreaDimensions { public double Width { get; set; } public double Depth { get; set; } public double Height { get; set; } }
public class MachineDimensions { public double Width { get; set; } public double Depth { get; set; } public double Height { get; set; } }
public class JogPreset { public string Label { get; set; } = string.Empty; public double FeedMmMin { get; set; } public double DistanceMm { get; set; } }

public class MotionDefaultsSection
{
    public Dictionary<string, double> StepsPerMm { get; set; } = new();
    public Dictionary<string, double> MaxFeedMmMin { get; set; } = new();
    public Dictionary<string, double>? MaxAccelMmSec2 { get; set; }
    public List<JogPreset> JogPresets { get; set; } = new();
}

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

public class CapabilitiesSection
{
    public MotionCapabilities Motion { get; set; } = new();
    public ExecutionCapabilities Execution { get; set; } = new();
    public ProtocolCapabilities Protocol { get; set; } = new();
    public VisualizationCapabilities Visualization { get; set; } = new();
    public FileHandlingCapabilities FileHandling { get; set; } = new();
}

public class MotionCapabilities { public bool Homing { get; set; } public bool ZHoming { get; set; } public bool CombinedXYHoming { get; set; } public bool RelativeMoves { get; set; } public bool AbsoluteMoves { get; set; } public bool Pause { get; set; } public bool Resume { get; set; } public bool Stop { get; set; } public bool Park { get; set; } public bool CenterMove { get; set; } public bool WorkOffset { get; set; } public bool JogContinuous { get; set; } public bool JogStep { get; set; } }
public class ExecutionCapabilities { public bool RealExecution { get; set; } public bool Simulation { get; set; } public bool PreviewPlayback { get; set; } public bool DryRun { get; set; } public bool FileRun { get; set; } public bool Frame { get; set; } public bool BoundingBoxPreview { get; set; } public bool LiveReportedPosition { get; set; } public bool EstimatedPositionOnly { get; set; } public bool ToolpathPreview { get; set; } public bool ProgressTracking { get; set; } }
public class ProtocolCapabilities { public bool Handshake { get; set; } public bool Acknowledgements { get; set; } public bool AlarmReporting { get; set; } public bool AlarmReset { get; set; } public bool StatusQuery { get; set; } public bool PositionQuery { get; set; } public bool MotorEnable { get; set; } public bool MotorDisable { get; set; } public bool FeedHold { get; set; } public bool SoftReset { get; set; } }
public class VisualizationCapabilities { public bool MachineVisualization { get; set; } public bool TopView2D { get; set; } public bool Perspective3D { get; set; } public bool KinematicsAnimation { get; set; } public bool RealTimePositionDisplay { get; set; } }
public class FileHandlingCapabilities { public bool LocalFileRun { get; set; } public bool StreamingExecution { get; set; } public bool GcodeValidation { get; set; } public bool MultipleFileFormats { get; set; } }

public class FileSupportSection
{
    public List<string> SupportedInputFileTypes { get; set; } = new();
    public GcodeDialect GcodeDialect { get; set; } = GcodeDialect.Generic;
    public List<OperationType> SupportedOperationTypes { get; set; } = new();
}

public class VisualizationSection
{
    public VisualizationType VisualizationType { get; set; } = VisualizationType.Generic2D;
    public KinematicsType KinematicsType { get; set; } = KinematicsType.Unknown;
    public CoordinateMode CoordinatePresentationMode { get; set; } = CoordinateMode.BottomLeft;
    public MachineShapeHint MachineShapeHint { get; set; } = MachineShapeHint.Rectangular;
    public ViewMode DefaultViewMode { get; set; } = ViewMode.Top2D;
}

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

public class BuiltInProfileRules { public bool IsEditable { get; set; } public bool IsDeletable { get; set; } public bool IsDuplicatable { get; set; } = true; public string DuplicateProducesType { get; set; } = "User"; }
public class UserProfileRules { public bool IsEditable { get; set; } = true; public bool IsDeletable { get; set; } = true; public bool IsDuplicatable { get; set; } = true; public string DuplicateProducesType { get; set; } = "User"; public int? MaxUserProfiles { get; set; } }

public class MachineDefinitionCacheStore
{
    public DateTime LastSyncedAt { get; set; }
    public string LastSyncStatus { get; set; } = "Never synced.";
    public List<MachineCategory> Categories { get; set; } = new();
    public List<MachineFamily> Families { get; set; } = new();
    public List<MachineDefinitionSummary> DefinitionSummaries { get; set; } = new();
    public List<MachineDefinition> Definitions { get; set; } = new();
}

public class RuntimeProfile
{
    public string RuntimeProfileId { get; set; } = Guid.NewGuid().ToString("N");
    public string ProfileName { get; set; } = "Machine Profile";
    public RuntimeProfileType ProfileType { get; set; } = RuntimeProfileType.User;
    public Guid MachineDefinitionId { get; set; }
    public string MachineDefinitionVersion { get; set; } = string.Empty;
    public MachineDefinition? DefinitionSnapshot { get; set; }
    public RuntimeProfileOverrides Overrides { get; set; } = new();
    public DefinitionCompatibilityState CompatibilityState { get; set; } = DefinitionCompatibilityState.Current;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class RuntimeProfileOverrides
{
    public DriverType? DriverType { get; set; }
    public int? BaudRate { get; set; }
    public string? Port { get; set; }
    public Dictionary<string, double>? StepsPerMm { get; set; }
    public List<double>? JogPresets { get; set; }
    public string? Notes { get; set; }
}

public class RuntimeProfileStore
{
    public string? ActiveRuntimeProfileId { get; set; }
    public List<RuntimeProfile> Profiles { get; set; } = new();
}

public class ActiveMachineContext
{
    public RuntimeProfile? RuntimeProfile { get; set; }
    public MachineDefinition? MachineDefinition { get; set; }
    public DriverType DriverType { get; set; } = DriverType.Unknown;
    public CncDriverType CncDriverType { get; set; } = CncDriverType.ArduinoSerial;
    public CncDriverCapabilities DriverCapabilities { get; set; } = new();
    public CapabilitiesSection EffectiveCapabilities { get; set; } = new();
    public string RuntimeUiVariant { get; set; } = "generic-v1";
    public bool IsOfflineSnapshot { get; set; }
    public string StatusText { get; set; } = "No active machine context.";
}
