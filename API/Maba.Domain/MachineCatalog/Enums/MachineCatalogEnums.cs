namespace Maba.Domain.MachineCatalog.Enums;

public enum DriverType
{
    ArduinoSerial,
    GRBL,
    Marlin,
    RepRap,
    MabaCustom,
    Simulated,
    NetworkMaba,
    Unknown
}

public enum FirmwareProtocol
{
    GRBL_1_1,
    GRBL_0_9,
    Marlin_2,
    Marlin_1,
    RepRap,
    MabaProtocol,
    Custom,
    Unknown
}

public enum KinematicsType
{
    MovingGantryXY,
    FixedGantryMovingBed,
    CoreXY,
    Delta,
    LaserFlatbed,
    CartesianPrinter,
    Scara,
    Unknown
}

public enum VisualizationType
{
    CncTopDown2D,
    LaserFlatbed2D,
    Printer3DCartesian,
    Printer3DDelta,
    Generic2D,
    Generic3D
}

public enum SetupMode
{
    RealOnly,
    SimulationOnly,
    RealAndSimulation
}

public enum ConnectionType
{
    Serial,
    USB,
    Network,
    Bluetooth,
    Simulated
}

public enum HomeOriginConvention
{
    FrontLeft,
    FrontRight,
    BackLeft,
    BackRight,
    Center,
    Custom
}

public enum OperationType
{
    Milling,
    Engraving,
    Drilling,
    LaserCutting,
    LaserEngraving,
    FDMPrint,
    ResinPrint,
    Plotting
}

public enum GcodeDialect
{
    GRBL,
    Marlin,
    RepRap,
    MabaCustom,
    Generic
}

public enum ViewMode
{
    Top2D,
    Perspective3D,
    Side2D,
    Isometric
}

public enum CoordinateMode
{
    TopLeft,
    BottomLeft,
    Center
}

public enum MachineShapeHint
{
    Rectangular,
    Delta,
    Polar,
    Articulated
}

public enum AxisId
{
    X,
    Y,
    Z,
    A,
    B,
    C
}

public enum AxisRole
{
    Primary,
    Secondary,
    Vertical,
    Rotational,
    Extruder,
    Generic
}

public enum Direction
{
    Normal,
    Inverted
}

public enum OverrideField
{
    DriverType,
    BaudRate,
    Port,
    StepsPerMm,
    MaxFeed,
    MaxAccel,
    JogPresets,
    SafeZ,
    ParkPosition,
    WorkOffset,
    Visualization,
    Notes
}

public enum ConstraintType
{
    Range,
    AllowedValues,
    MaxDeltaPercent,
    FreeText,
    None
}
