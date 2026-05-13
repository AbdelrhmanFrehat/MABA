namespace MabaControlCenter.Models;

public enum CncCapabilityConfidence
{
    Unknown,
    Inferred,
    Verified
}

public enum CncFirmwareCompatibilityStatus
{
    Unknown,
    Compatible,
    CompatibleWithWarnings,
    Incompatible
}

public class CncProtocolVersion
{
    public string ProtocolName { get; set; } = "Unknown";
    public string RawVersion { get; set; } = "Unknown";
    public int? Major { get; set; }
    public int? Minor { get; set; }
}

public class CncFirmwareCapabilities
{
    public bool SupportsStatusQuery { get; set; }
    public bool SupportsUnlock { get; set; }
    public bool SupportsMotorEnable { get; set; }
    public bool SupportsMotorDisable { get; set; }
    public bool SupportsHoming { get; set; }
    public bool SupportsJog { get; set; }
    public bool SupportsG0G1 { get; set; }
    public bool SupportsG2G3 { get; set; }
    public bool SupportsSpindleOnOff { get; set; }
    public bool SupportsSpindleSpeed { get; set; }
    public bool SupportsFeedHold { get; set; }
    public bool SupportsSoftwareStop { get; set; }
    public bool SupportsWorkOffsets { get; set; }
    public bool SupportsLimitReporting { get; set; }
    public bool SupportsPositionReporting { get; set; }
    public bool SupportsFirmwareUpload { get; set; }
    public List<string> SupportedAxes { get; set; } = new();
    public decimal? WorkspaceLimitX { get; set; }
    public decimal? WorkspaceLimitY { get; set; }
    public decimal? WorkspaceLimitZ { get; set; }

    public CncFirmwareCapabilities Clone()
    {
        return new CncFirmwareCapabilities
        {
            SupportsStatusQuery = SupportsStatusQuery,
            SupportsUnlock = SupportsUnlock,
            SupportsMotorEnable = SupportsMotorEnable,
            SupportsMotorDisable = SupportsMotorDisable,
            SupportsHoming = SupportsHoming,
            SupportsJog = SupportsJog,
            SupportsG0G1 = SupportsG0G1,
            SupportsG2G3 = SupportsG2G3,
            SupportsSpindleOnOff = SupportsSpindleOnOff,
            SupportsSpindleSpeed = SupportsSpindleSpeed,
            SupportsFeedHold = SupportsFeedHold,
            SupportsSoftwareStop = SupportsSoftwareStop,
            SupportsWorkOffsets = SupportsWorkOffsets,
            SupportsLimitReporting = SupportsLimitReporting,
            SupportsPositionReporting = SupportsPositionReporting,
            SupportsFirmwareUpload = SupportsFirmwareUpload,
            SupportedAxes = SupportedAxes.ToList(),
            WorkspaceLimitX = WorkspaceLimitX,
            WorkspaceLimitY = WorkspaceLimitY,
            WorkspaceLimitZ = WorkspaceLimitZ
        };
    }
}

public class CncFirmwareIdentity
{
    public string FirmwareName { get; set; } = "Unknown";
    public string FirmwareVersion { get; set; } = "Unknown";
    public string ProtocolName { get; set; } = "Unknown";
    public CncProtocolVersion ProtocolVersion { get; set; } = new();
    public string? MachineId { get; set; }
    public string? BuildDate { get; set; }
    public CncCapabilityConfidence Confidence { get; set; } = CncCapabilityConfidence.Unknown;
    public CncFirmwareCapabilities Capabilities { get; set; } = new();
    public List<string> FirmwareWarnings { get; set; } = new();
    public List<string> CompatibilityErrors { get; set; } = new();

    public bool IsKnown => !string.Equals(FirmwareName, "Unknown", StringComparison.OrdinalIgnoreCase);

    public CncFirmwareIdentity Clone()
    {
        return new CncFirmwareIdentity
        {
            FirmwareName = FirmwareName,
            FirmwareVersion = FirmwareVersion,
            ProtocolName = ProtocolName,
            ProtocolVersion = new CncProtocolVersion
            {
                ProtocolName = ProtocolVersion.ProtocolName,
                RawVersion = ProtocolVersion.RawVersion,
                Major = ProtocolVersion.Major,
                Minor = ProtocolVersion.Minor
            },
            MachineId = MachineId,
            BuildDate = BuildDate,
            Confidence = Confidence,
            Capabilities = Capabilities.Clone(),
            FirmwareWarnings = FirmwareWarnings.ToList(),
            CompatibilityErrors = CompatibilityErrors.ToList()
        };
    }
}

public class CncFirmwareCompatibilityResult
{
    public CncFirmwareCompatibilityStatus Status { get; set; } = CncFirmwareCompatibilityStatus.Unknown;
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();

    public bool IsCompatible => Status is CncFirmwareCompatibilityStatus.Compatible or CncFirmwareCompatibilityStatus.CompatibleWithWarnings;

    public CncFirmwareCompatibilityResult Clone()
    {
        return new CncFirmwareCompatibilityResult
        {
            Status = Status,
            Warnings = Warnings.ToList(),
            Errors = Errors.ToList()
        };
    }
}

public class CncFirmwareHandshakeResult
{
    public CncFirmwareIdentity Identity { get; set; } = new();
    public string RawHandshakeText { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public string Summary { get; set; } = "No firmware handshake has been performed.";
}
