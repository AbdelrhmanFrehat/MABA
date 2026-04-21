// ── Enums ─────────────────────────────────────────────────────────────────

export type DriverType = 'ArduinoSerial' | 'GRBL' | 'Marlin' | 'RepRap' | 'MabaCustom' | 'Simulated' | 'NetworkMaba' | 'Unknown';
export type FirmwareProtocol = 'GRBL_1_1' | 'GRBL_0_9' | 'Marlin_2' | 'Marlin_1' | 'RepRap' | 'MabaProtocol' | 'Custom' | 'Unknown';
export type KinematicsType = 'MovingGantryXY' | 'FixedGantryMovingBed' | 'CoreXY' | 'Delta' | 'LaserFlatbed' | 'CartesianPrinter' | 'Scara' | 'Unknown';
export type VisualizationType = 'CncTopDown2D' | 'LaserFlatbed2D' | 'Printer3DCartesian' | 'Printer3DDelta' | 'Generic2D' | 'Generic3D';
export type SetupMode = 'RealOnly' | 'SimulationOnly' | 'RealAndSimulation';
export type ConnectionType = 'Serial' | 'USB' | 'Network' | 'Bluetooth' | 'Simulated';
export type HomeOriginConvention = 'FrontLeft' | 'FrontRight' | 'BackLeft' | 'BackRight' | 'Center' | 'Custom';
export type OperationType = 'Milling' | 'Engraving' | 'Drilling' | 'LaserCutting' | 'LaserEngraving' | 'FDMPrint' | 'ResinPrint' | 'Plotting';
export type GcodeDialect = 'GRBL' | 'Marlin' | 'RepRap' | 'MabaCustom' | 'Generic';
export type ViewMode = 'Top2D' | 'Perspective3D' | 'Side2D' | 'Isometric';
export type CoordinateMode = 'TopLeft' | 'BottomLeft' | 'Center';
export type MachineShapeHint = 'Rectangular' | 'Delta' | 'Polar' | 'Articulated';
export type AxisId = 'X' | 'Y' | 'Z' | 'A' | 'B' | 'C';
export type AxisRole = 'Primary' | 'Secondary' | 'Vertical' | 'Rotational' | 'Extruder' | 'Generic';
export type Direction = 'Normal' | 'Inverted';
export type OverrideField = 'DriverType' | 'BaudRate' | 'Port' | 'StepsPerMm' | 'MaxFeed' | 'MaxAccel' | 'JogPresets' | 'SafeZ' | 'ParkPosition' | 'WorkOffset' | 'Visualization' | 'Notes';
export type ConstraintType = 'Range' | 'AllowedValues' | 'MaxDeltaPercent' | 'FreeText' | 'None';

// ── Entities ──────────────────────────────────────────────────────────────

export interface MachineCategory {
    id: string;
    code: string;
    displayNameEn: string;
    displayNameAr: string;
    descriptionEn?: string;
    descriptionAr?: string;
    iconKey?: string;
    sortOrder: number;
    isActive: boolean;
    createdAt: string;
    updatedAt?: string;
}

export interface MachineFamily {
    id: string;
    categoryId: string;
    categoryDisplayNameEn?: string;
    code: string;
    displayNameEn: string;
    displayNameAr: string;
    descriptionEn?: string;
    descriptionAr?: string;
    manufacturer: string;
    logoUrl?: string;
    isActive: boolean;
    sortOrder: number;
    createdAt: string;
    updatedAt?: string;
}

// ── Sections ──────────────────────────────────────────────────────────────

export interface JogPreset {
    label: string;
    feedMmMin: number;
    distanceMm: number;
}

export interface WorkAreaDimensions { width: number; depth: number; height: number; }
export interface MachineDimensions   { width: number; depth: number; height: number; }

export interface RuntimeBindingSection {
    defaultDriverType: DriverType;
    supportedDriverTypes: DriverType[];
    firmwareProtocol: FirmwareProtocol;
    supportedSetupModes: SetupMode[];
    visualizationType: VisualizationType;
    kinematicsType: KinematicsType;
    runtimeUiVariant: string;
}

export interface AxisConfigSection {
    axisCount: number;
    supportedAxes: AxisId[];
    axisRoles: Record<string, AxisRole>;
    axisDirections: Record<string, Direction>;
    homingSupport: Record<string, boolean>;
    homeOriginConvention: HomeOriginConvention;
    workCoordinateSupport: boolean;
    machineCoordinateSupport: boolean;
    relativeMoveSupport: boolean;
    absoluteMoveSupport: boolean;
}

export interface WorkspaceSection {
    maxTravelMm: Record<string, number>;
    minTravelMm?: Record<string, number>;
    workAreaMm: WorkAreaDimensions;
    machineDimensionsMm?: MachineDimensions;
    safeZHeightMm?: number;
    parkPositionMm?: Record<string, number>;
}

export interface MotionDefaultsSection {
    stepsPerMm: Record<string, number>;
    maxFeedMmMin: Record<string, number>;
    maxAccelMmSec2?: Record<string, number>;
    jogPresets: JogPreset[];
}

export interface ConnectionDefaultsSection {
    defaultBaudRate: number;
    supportedBaudRates: number[];
    supportedConnectionTypes: ConnectionType[];
    requiresHandshake: boolean;
    commandTerminator: string;
    responseAckPattern?: string;
    protocolNotes?: string;
}

export interface MotionCapabilities { homing: boolean; zHoming: boolean; combinedXYHoming: boolean; relativeMoves: boolean; absoluteMoves: boolean; pause: boolean; resume: boolean; stop: boolean; park: boolean; centerMove: boolean; workOffset: boolean; jogContinuous: boolean; jogStep: boolean; }
export interface ExecutionCapabilities { realExecution: boolean; simulation: boolean; previewPlayback: boolean; dryRun: boolean; fileRun: boolean; frame: boolean; boundingBoxPreview: boolean; liveReportedPosition: boolean; estimatedPositionOnly: boolean; toolpathPreview: boolean; progressTracking: boolean; }
export interface ProtocolCapabilities { handshake: boolean; acknowledgements: boolean; alarmReporting: boolean; alarmReset: boolean; statusQuery: boolean; positionQuery: boolean; motorEnable: boolean; motorDisable: boolean; feedHold: boolean; softReset: boolean; }
export interface VisualizationCapabilities { machineVisualization: boolean; topView2D: boolean; perspective3D: boolean; kinematicsAnimation: boolean; realTimePositionDisplay: boolean; }
export interface FileHandlingCapabilities { localFileRun: boolean; streamingExecution: boolean; gcodeValidation: boolean; multipleFileFormats: boolean; }

export interface CapabilitiesSection {
    motion: MotionCapabilities;
    execution: ExecutionCapabilities;
    protocol: ProtocolCapabilities;
    visualization: VisualizationCapabilities;
    fileHandling: FileHandlingCapabilities;
}

export interface FileSupportSection {
    supportedInputFileTypes: string[];
    gcodeDialect: GcodeDialect;
    supportedOperationTypes: OperationType[];
}

export interface VisualizationSection {
    visualizationType: VisualizationType;
    kinematicsType: KinematicsType;
    coordinatePresentationMode: CoordinateMode;
    machineShapeHint: MachineShapeHint;
    defaultViewMode: ViewMode;
}

export interface OverrideConstraint {
    field: OverrideField;
    constraintType: ConstraintType;
    minValue?: number;
    maxValue?: number;
    allowedValues?: string[];
    maxDeltaPercent?: number;
}

export interface BuiltInProfileRules { isEditable: boolean; isDeletable: boolean; isDuplicatable: boolean; duplicateProducesType: string; }
export interface UserProfileRules { isEditable: boolean; isDeletable: boolean; isDuplicatable: boolean; duplicateProducesType: string; maxUserProfiles?: number; }

export interface ProfileRulesSection {
    allowedOverrides: OverrideField[];
    overrideConstraints: OverrideConstraint[];
    builtInProfileRules: BuiltInProfileRules;
    userProfileRules: UserProfileRules;
}

// ── MachineDefinition ─────────────────────────────────────────────────────

export interface MachineDefinitionSummary {
    id: string;
    code: string;
    version: string;
    categoryId: string;
    categoryDisplayNameEn?: string;
    familyId: string;
    familyDisplayNameEn?: string;
    displayNameEn: string;
    displayNameAr: string;
    manufacturer: string;
    isActive: boolean;
    isPublic: boolean;
    isDeprecated: boolean;
    deprecationNote?: string;
    sortOrder: number;
    releasedAt?: string;
    imageUrl?: string;
    thumbnailUrl?: string;
    tags: string[];
    defaultDriverType: string;
    supportedSetupModes: string[];
    runtimeUiVariant: string;
    createdAt: string;
    updatedAt?: string;
}

export interface MachineDefinition {
    id: string;
    code: string;
    version: string;
    revisionNote?: string;
    categoryId: string;
    categoryDisplayNameEn?: string;
    familyId: string;
    familyDisplayNameEn?: string;
    displayNameEn: string;
    displayNameAr: string;
    descriptionEn?: string;
    descriptionAr?: string;
    manufacturer: string;
    tags: string[];
    isActive: boolean;
    isPublic: boolean;
    isDeprecated: boolean;
    deprecationNote?: string;
    sortOrder: number;
    releasedAt?: string;
    imageUrl?: string;
    thumbnailUrl?: string;
    internalNotes?: string;
    createdAt: string;
    updatedAt?: string;
    runtimeBinding: RuntimeBindingSection;
    axisConfig: AxisConfigSection;
    workspace: WorkspaceSection;
    motionDefaults: MotionDefaultsSection;
    connectionDefaults: ConnectionDefaultsSection;
    capabilities: CapabilitiesSection;
    fileSupport: FileSupportSection;
    visualization: VisualizationSection;
    profileRules: ProfileRulesSection;
}
