using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class CncProfileService : ICncProfileService
{
    private static readonly JsonSerializerOptions JsonOptions = MachinePlatformJson.CreateOptions();
    private readonly string _profilesFilePath;
    private readonly string _legacyConfigFilePath;
    private CncMachineProfile? _activeProfile;

    public CncProfileService()
    {
        var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MabaControlCenter");
        _profilesFilePath = Path.Combine(baseDir, "cnc-profiles.json");
        _legacyConfigFilePath = Path.Combine(baseDir, "cnc-config.json");
        Profiles = new ObservableCollection<CncMachineProfile>();

        var store = LoadStore();
        foreach (var profile in store.Profiles)
            Profiles.Add(profile);

        _activeProfile = Profiles.FirstOrDefault(p => p.ProfileId == store.ActiveProfileId) ?? Profiles.First();
    }

    public ObservableCollection<CncMachineProfile> Profiles { get; }
    public CncMachineProfile ActiveProfile => _activeProfile ?? Profiles.First();
    public event EventHandler? ActiveProfileChanged;
    public event EventHandler? ProfilesChanged;

    public void SaveProfile(CncMachineProfile profile)
    {
        var existing = Profiles.FirstOrDefault(p => p.ProfileId == profile.ProfileId);
        if (existing == null)
        {
            var newProfile = Clone(profile);
            newProfile.IsBuiltIn = false;
            newProfile.IsDefault = false;
            Profiles.Add(newProfile);
        }
        else
        {
            if (existing.IsBuiltIn)
                throw new InvalidOperationException("System CNC profiles are read-only. Duplicate the profile to create an editable copy.");

            var index = Profiles.IndexOf(existing);
            var updatedProfile = Clone(profile);
            updatedProfile.IsBuiltIn = false;
            updatedProfile.IsDefault = false;
            Profiles[index] = updatedProfile;
            if (_activeProfile?.ProfileId == existing.ProfileId)
                _activeProfile = Profiles[index];
        }

        SaveStore();
        ProfilesChanged?.Invoke(this, EventArgs.Empty);
        if (_activeProfile?.ProfileId == profile.ProfileId)
            ActiveProfileChanged?.Invoke(this, EventArgs.Empty);
    }

    public CncMachineProfile CreateProfileFromMachineDefinition(MachineDefinition definition, string profileName, DriverType? driverOverride = null, RuntimeProfileType profileType = RuntimeProfileType.User)
    {
        var profile = MapDefinitionToProfile(definition, profileName, driverOverride);
        profile.IsBuiltIn = profileType == RuntimeProfileType.System;
        profile.IsDefault = false;
        Profiles.Add(profile);
        if (_activeProfile == null)
            _activeProfile = profile;
        SaveStore();
        ProfilesChanged?.Invoke(this, EventArgs.Empty);
        ActiveProfileChanged?.Invoke(this, EventArgs.Empty);
        return Clone(profile);
    }

    public CncMachineProfile DuplicateProfile(string profileId)
    {
        var source = Profiles.First(p => p.ProfileId == profileId);
        var clone = Clone(source);
        clone.ProfileId = Guid.NewGuid().ToString("N");
        clone.ProfileName = $"{source.ProfileName} Copy";
        clone.IsDefault = false;
        clone.IsBuiltIn = false;
        Profiles.Add(clone);
        SaveStore();
        ProfilesChanged?.Invoke(this, EventArgs.Empty);
        return Clone(clone);
    }

    public bool DeleteProfile(string profileId)
    {
        var existing = Profiles.FirstOrDefault(p => p.ProfileId == profileId);
        if (existing == null || existing.IsBuiltIn || existing.IsDefault || Profiles.Count <= 1)
            return false;

        var wasActive = _activeProfile?.ProfileId == profileId;
        Profiles.Remove(existing);
        if (wasActive)
        {
            _activeProfile = Profiles.First();
            ActiveProfileChanged?.Invoke(this, EventArgs.Empty);
        }

        SaveStore();
        ProfilesChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public void SetActiveProfile(string profileId)
    {
        var profile = Profiles.First(p => p.ProfileId == profileId);
        if (_activeProfile?.ProfileId == profile.ProfileId)
            return;

        _activeProfile = profile;
        SaveStore();
        ActiveProfileChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RestoreDefaultProfiles()
    {
        Profiles.Clear();
        foreach (var profile in CreateDefaultProfiles())
            Profiles.Add(profile);

        _activeProfile = Profiles.First();
        SaveStore();
        ProfilesChanged?.Invoke(this, EventArgs.Empty);
        ActiveProfileChanged?.Invoke(this, EventArgs.Empty);
    }

    private CncProfileStore LoadStore()
    {
        try
        {
            if (File.Exists(_profilesFilePath))
            {
                var json = File.ReadAllText(_profilesFilePath);
                var loaded = JsonSerializer.Deserialize<CncProfileStore>(json, JsonOptions);
                if (loaded?.Profiles?.Count > 0)
                {
                    var hadSimulationProfile = loaded.Profiles.Any(p => p.DriverType == CncDriverType.Simulated);
                    var changedBuiltIns = EnsureBuiltInProfiles(loaded.Profiles);
                    if (changedBuiltIns)
                        SaveStore(loaded);

                    if (!hadSimulationProfile && !loaded.Profiles.Any(p => p.ProfileId == loaded.ActiveProfileId && p.DriverType == CncDriverType.Simulated))
                    {
                        loaded.ActiveProfileId = loaded.Profiles.First(p => p.DriverType == CncDriverType.Simulated).ProfileId;
                        SaveStore(loaded);
                    }
                    return loaded;
                }
            }
        }
        catch
        {
            // Ignore bad profile persistence and rebuild defaults.
        }

        var defaults = CreateDefaultProfiles();
        var store = new CncProfileStore
        {
            ActiveProfileId = defaults.First(p => p.DriverType == CncDriverType.Simulated).ProfileId,
            Profiles = defaults
        };
        SaveStore(store);
        return store;
    }

    private List<CncMachineProfile> CreateDefaultProfiles()
    {
        var hardwareProfile = LoadLegacyProfile() ?? new CncMachineProfile
        {
            IsDefault = true,
            IsBuiltIn = true,
            ProfileName = "Arduino CNC - Default",
            Description = "Default Arduino-based CNC profile for the current machine."
        };

        hardwareProfile.IsDefault = true;
        hardwareProfile.IsBuiltIn = true;
        hardwareProfile.ProfileName = string.IsNullOrWhiteSpace(hardwareProfile.ProfileName) ? "Arduino CNC - Default" : hardwareProfile.ProfileName;

        var simulatedProfile = new CncMachineProfile
        {
            ProfileName = "Simulated CNC - Offline",
            Description = "Offline CNC simulation profile for development, demos, and remote testing without hardware.",
            DriverType = CncDriverType.Simulated,
            IsBuiltIn = true,
            Notes = "Uses the simulated CNC driver and does not require a COM port.",
            JogPresets = new List<decimal> { 0.1m, 1m, 5m, 10m },
            VisualizationWidthMm = hardwareProfile.XLimitMm - hardwareProfile.XMinMm,
            VisualizationHeightMm = hardwareProfile.YLimitMm - hardwareProfile.YMinMm,
            VisualizationDepthMm = hardwareProfile.ZLimitMm - hardwareProfile.ZMinMm
        };

        return new List<CncMachineProfile> { hardwareProfile, simulatedProfile };
    }

    private bool EnsureBuiltInProfiles(List<CncMachineProfile> profiles)
    {
        var changed = false;
        foreach (var profile in profiles.Where(IsSystemProfile))
        {
            if (!profile.IsBuiltIn)
            {
                profile.IsBuiltIn = true;
                changed = true;
            }
        }

        if (!profiles.Any(p => p.DriverType == CncDriverType.Simulated))
        {
            profiles.Add(new CncMachineProfile
            {
                ProfileName = "Simulated CNC - Offline",
                Description = "Offline CNC simulation profile for development, demos, and remote testing without hardware.",
                DriverType = CncDriverType.Simulated,
                IsBuiltIn = true,
                Notes = "Uses the simulated CNC driver and does not require a COM port.",
                JogPresets = new List<decimal> { 0.1m, 1m, 5m, 10m }
            });
            changed = true;
        }

        return changed;
    }

    private static bool IsSystemProfile(CncMachineProfile profile)
    {
        return profile.IsDefault
               || string.Equals(profile.ProfileName, "Arduino CNC - Default", StringComparison.OrdinalIgnoreCase)
               || string.Equals(profile.ProfileName, "Simulated CNC - Offline", StringComparison.OrdinalIgnoreCase);
    }

    private CncMachineProfile? LoadLegacyProfile()
    {
        try
        {
            if (!File.Exists(_legacyConfigFilePath))
                return null;

            var json = File.ReadAllText(_legacyConfigFilePath);
            var legacy = JsonSerializer.Deserialize<CncMachineConfig>(json, JsonOptions);
            return legacy == null ? null : CncMachineProfile.FromConfig(legacy);
        }
        catch
        {
            return null;
        }
    }

    private void SaveStore()
    {
        SaveStore(new CncProfileStore
        {
            ActiveProfileId = _activeProfile?.ProfileId,
            Profiles = Profiles.Select(Clone).ToList()
        });
    }

    private void SaveStore(CncProfileStore store)
    {
        try
        {
            var dir = Path.GetDirectoryName(_profilesFilePath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(store, JsonOptions);
            File.WriteAllText(_profilesFilePath, json);
        }
        catch
        {
            // Local profile persistence should not break runtime behavior.
        }
    }

    private static CncMachineProfile Clone(CncMachineProfile profile)
    {
        return JsonSerializer.Deserialize<CncMachineProfile>(JsonSerializer.Serialize(profile, JsonOptions), JsonOptions) ?? new CncMachineProfile();
    }

    private static CncMachineProfile MapDefinitionToProfile(MachineDefinition definition, string profileName, DriverType? driverOverride)
    {
        var driver = driverOverride ?? definition.RuntimeBinding.DefaultDriverType;
        var max = definition.Workspace.MaxTravelMm;
        var min = definition.Workspace.MinTravelMm ?? new Dictionary<string, double>();
        var steps = definition.MotionDefaults.StepsPerMm;

        return new CncMachineProfile
        {
            ProfileName = string.IsNullOrWhiteSpace(profileName) ? definition.DisplayNameEn : profileName.Trim(),
            MachineType = definition.CategoryDisplayNameEn ?? "CNC",
            Description = definition.DescriptionEn ?? definition.DisplayNameEn,
            Notes = definition.DeprecationNote ?? string.Empty,
            MachineDefinitionId = definition.Id,
            MachineDefinitionVersion = definition.Version,
            DefinitionSnapshot = CloneDefinition(definition),
            CompatibilityState = DefinitionCompatibilityState.Current,
            DriverType = ToCncDriverType(driver),
            BaudRate = definition.ConnectionDefaults.DefaultBaudRate <= 0 ? 115200 : definition.ConnectionDefaults.DefaultBaudRate,
            XStepsPerMm = ToDecimal(steps, "X", 80m),
            YStepsPerMm = ToDecimal(steps, "Y", 80m),
            ZStepsPerMm = ToDecimal(steps, "Z", 400m),
            XMinMm = ToDecimal(min, "X", 0m),
            YMinMm = ToDecimal(min, "Y", 0m),
            ZMinMm = ToDecimal(min, "Z", 0m),
            XLimitMm = ToDecimal(max, "X", definition.Workspace.WorkAreaMm.Width > 0 ? (decimal)definition.Workspace.WorkAreaMm.Width : 300m),
            YLimitMm = ToDecimal(max, "Y", definition.Workspace.WorkAreaMm.Depth > 0 ? (decimal)definition.Workspace.WorkAreaMm.Depth : 300m),
            ZLimitMm = ToDecimal(max, "Z", definition.Workspace.WorkAreaMm.Height > 0 ? (decimal)definition.Workspace.WorkAreaMm.Height : 100m),
            HomeOriginConvention = ToCncHomeOrigin(definition.AxisConfig.HomeOriginConvention),
            HomeXEnabled = definition.AxisConfig.HomingSupport.TryGetValue("X", out var homeX) && homeX,
            HomeYEnabled = definition.AxisConfig.HomingSupport.TryGetValue("Y", out var homeY) && homeY,
            HomeZEnabled = definition.AxisConfig.HomingSupport.TryGetValue("Z", out var homeZ) && homeZ,
            SupportsXAxis = definition.AxisConfig.SupportedAxes.Contains(AxisId.X),
            SupportsYAxis = definition.AxisConfig.SupportedAxes.Contains(AxisId.Y),
            SupportsZAxis = definition.AxisConfig.SupportedAxes.Contains(AxisId.Z),
            SoftLimitsEnabled = true,
            VisualizationWidthMm = definition.Workspace.WorkAreaMm.Width > 0 ? (decimal)definition.Workspace.WorkAreaMm.Width : ToDecimal(max, "X", 300m),
            VisualizationHeightMm = definition.Workspace.WorkAreaMm.Depth > 0 ? (decimal)definition.Workspace.WorkAreaMm.Depth : ToDecimal(max, "Y", 300m),
            VisualizationDepthMm = definition.Workspace.WorkAreaMm.Height > 0 ? (decimal)definition.Workspace.WorkAreaMm.Height : ToDecimal(max, "Z", 100m),
            JogPresets = definition.MotionDefaults.JogPresets.Count > 0
                ? definition.MotionDefaults.JogPresets.Select(p => (decimal)p.DistanceMm).Where(v => v > 0).Distinct().ToList()
                : new List<decimal> { 0.1m, 1m, 10m }
        };
    }

    private static CncDriverType ToCncDriverType(DriverType driverType)
        => driverType == DriverType.Simulated ? CncDriverType.Simulated : CncDriverType.ArduinoSerial;

    private static CncHomeOriginConvention ToCncHomeOrigin(MachineHomeOriginConvention origin)
        => origin is MachineHomeOriginConvention.BackLeft or MachineHomeOriginConvention.FrontLeft
            ? CncHomeOriginConvention.TopLeft
            : CncHomeOriginConvention.TopLeft;

    private static decimal ToDecimal(IReadOnlyDictionary<string, double> values, string key, decimal fallback)
        => values.TryGetValue(key, out var value) ? (decimal)value : fallback;

    private static MachineDefinition CloneDefinition(MachineDefinition definition)
        => JsonSerializer.Deserialize<MachineDefinition>(JsonSerializer.Serialize(definition, JsonOptions), JsonOptions) ?? definition;
}
