namespace MabaControlCenter.Models;

public class CncMachineProfile
{
    public string ProfileId { get; set; } = Guid.NewGuid().ToString("N");
    public string ProfileName { get; set; } = "Arduino CNC - Default";
    public string MachineType { get; set; } = "CNC";
    public string Description { get; set; } = "Default Arduino-based CNC machine profile.";
    public string Notes { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsBuiltIn { get; set; }
    public bool IsEditable => !IsBuiltIn;
    public Guid? MachineDefinitionId { get; set; }
    public string? MachineDefinitionVersion { get; set; }
    public MachineDefinition? DefinitionSnapshot { get; set; }
    public DefinitionCompatibilityState CompatibilityState { get; set; } = DefinitionCompatibilityState.Current;
    public CncDriverType DriverType { get; set; } = CncDriverType.ArduinoSerial;
    public int BaudRate { get; set; } = 115200;
    public decimal XStepsPerMm { get; set; } = 80m;
    public decimal YStepsPerMm { get; set; } = 80m;
    public decimal ZStepsPerMm { get; set; } = 400m;
    public decimal XMinMm { get; set; } = 0m;
    public decimal XLimitMm { get; set; } = 300m;
    public decimal YMinMm { get; set; } = 0m;
    public decimal YLimitMm { get; set; } = 300m;
    public decimal ZMinMm { get; set; } = 0m;
    public decimal ZLimitMm { get; set; } = 100m;
    public CncHomeOriginConvention HomeOriginConvention { get; set; } = CncHomeOriginConvention.TopLeft;
    public bool HomeXEnabled { get; set; } = true;
    public bool HomeYEnabled { get; set; } = true;
    public bool HomeZEnabled { get; set; }
    public bool SupportsXAxis { get; set; } = true;
    public bool SupportsYAxis { get; set; } = true;
    public bool SupportsZAxis { get; set; } = true;
    public bool RequireManualZZeroForCutting { get; set; } = true;
    public bool SoftLimitsEnabled { get; set; } = true;
    public decimal VisualizationWidthMm { get; set; } = 300m;
    public decimal VisualizationHeightMm { get; set; } = 300m;
    public decimal VisualizationDepthMm { get; set; } = 100m;
    public decimal SafeTravelZMm { get; set; } = 5m;
    public decimal MaxFeedXyMmPerMinute { get; set; } = 1200m;
    public decimal MaxRapidXyMmPerMinute { get; set; } = 1200m;
    public decimal MaxFeedZMmPerMinute { get; set; } = 300m;
    public decimal MaxPlungeZMmPerMinute { get; set; } = 200m;
    public decimal? ParkXMm { get; set; }
    public decimal? ParkYMm { get; set; }
    public decimal? ParkZMm { get; set; }
    public List<decimal> JogPresets { get; set; } = new() { 0.1m, 1m, 10m };

    public CncMachineConfig ToMachineConfig()
    {
        return new CncMachineConfig
        {
            BaudRate = BaudRate,
            XStepsPerMm = XStepsPerMm,
            YStepsPerMm = YStepsPerMm,
            ZStepsPerMm = ZStepsPerMm,
            XMinMm = XMinMm,
            XLimitMm = XLimitMm,
            YMinMm = YMinMm,
            YLimitMm = YLimitMm,
            ZMinMm = ZMinMm,
            ZLimitMm = ZLimitMm,
            HomeOriginConvention = HomeOriginConvention,
            HomeXEnabled = HomeXEnabled,
            HomeYEnabled = HomeYEnabled,
            HomeZEnabled = HomeZEnabled,
            SupportsXAxis = SupportsXAxis,
            SupportsYAxis = SupportsYAxis,
            SupportsZAxis = SupportsZAxis,
            RequireManualZZeroForCutting = RequireManualZZeroForCutting,
            SoftLimitsEnabled = SoftLimitsEnabled,
            DriverType = DriverType,
            JogPresets = JogPresets.ToList(),
            SafeTravelZMm = SafeTravelZMm,
            MaxFeedXyMmPerMinute = MaxFeedXyMmPerMinute,
            MaxRapidXyMmPerMinute = MaxRapidXyMmPerMinute,
            MaxFeedZMmPerMinute = MaxFeedZMmPerMinute,
            MaxPlungeZMmPerMinute = MaxPlungeZMmPerMinute,
            ParkXMm = ParkXMm,
            ParkYMm = ParkYMm,
            ParkZMm = ParkZMm,
            VisualizationWidthMm = VisualizationWidthMm,
            VisualizationHeightMm = VisualizationHeightMm,
            VisualizationDepthMm = VisualizationDepthMm
        };
    }

    public static CncMachineProfile FromConfig(CncMachineConfig config)
    {
        return new CncMachineProfile
        {
            ProfileName = "Arduino CNC - Default",
            MachineType = "CNC",
            Description = "Migrated default Arduino CNC profile.",
            IsDefault = true,
            IsBuiltIn = true,
            DriverType = config.DriverType,
            BaudRate = config.BaudRate,
            XStepsPerMm = config.XStepsPerMm,
            YStepsPerMm = config.YStepsPerMm,
            ZStepsPerMm = config.ZStepsPerMm,
            XMinMm = config.XMinMm,
            XLimitMm = config.XLimitMm,
            YMinMm = config.YMinMm,
            YLimitMm = config.YLimitMm,
            ZMinMm = config.ZMinMm,
            ZLimitMm = config.ZLimitMm,
            HomeOriginConvention = config.HomeOriginConvention,
            HomeXEnabled = config.HomeXEnabled,
            HomeYEnabled = config.HomeYEnabled,
            HomeZEnabled = config.HomeZEnabled,
            SupportsXAxis = config.SupportsXAxis,
            SupportsYAxis = config.SupportsYAxis,
            SupportsZAxis = config.SupportsZAxis,
            RequireManualZZeroForCutting = config.RequireManualZZeroForCutting,
            SoftLimitsEnabled = config.SoftLimitsEnabled,
            JogPresets = config.JogPresets.ToList(),
            SafeTravelZMm = config.SafeTravelZMm,
            MaxFeedXyMmPerMinute = config.MaxFeedXyMmPerMinute,
            MaxRapidXyMmPerMinute = config.MaxRapidXyMmPerMinute,
            MaxFeedZMmPerMinute = config.MaxFeedZMmPerMinute,
            MaxPlungeZMmPerMinute = config.MaxPlungeZMmPerMinute,
            ParkXMm = config.ParkXMm,
            ParkYMm = config.ParkYMm,
            ParkZMm = config.ParkZMm,
            VisualizationWidthMm = config.VisualizationWidthMm,
            VisualizationHeightMm = config.VisualizationHeightMm,
            VisualizationDepthMm = config.VisualizationDepthMm
        };
    }
}
