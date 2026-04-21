using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class RuntimeProfileService : IRuntimeProfileService
{
    private static readonly JsonSerializerOptions JsonOptions = MachinePlatformJson.CreateOptions();

    private readonly string _profilesFilePath;
    private RuntimeProfile? _activeProfile;

    public RuntimeProfileService()
    {
        var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MabaControlCenter");
        _profilesFilePath = Path.Combine(baseDir, "runtime-machine-profiles.json");
        Profiles = new ObservableCollection<RuntimeProfile>();
        LoadStore();
    }

    public ObservableCollection<RuntimeProfile> Profiles { get; }
    public RuntimeProfile? ActiveProfile => _activeProfile;
    public event EventHandler? ProfilesChanged;
    public event EventHandler? ActiveProfileChanged;

    public RuntimeProfile CreateFromDefinition(MachineDefinition definition, string profileName, RuntimeProfileType profileType, DriverType? driverOverride = null)
    {
        var profile = new RuntimeProfile
        {
            ProfileName = string.IsNullOrWhiteSpace(profileName) ? definition.DisplayNameEn : profileName.Trim(),
            ProfileType = profileType,
            MachineDefinitionId = definition.Id,
            MachineDefinitionVersion = definition.Version,
            DefinitionSnapshot = Clone(definition),
            CompatibilityState = DefinitionCompatibilityState.Current,
            Overrides = new RuntimeProfileOverrides
            {
                DriverType = IsOverrideAllowed(definition, OverrideField.DriverType) ? driverOverride : null,
                BaudRate = IsOverrideAllowed(definition, OverrideField.BaudRate) ? definition.ConnectionDefaults.DefaultBaudRate : null,
                StepsPerMm = IsOverrideAllowed(definition, OverrideField.StepsPerMm) ? definition.MotionDefaults.StepsPerMm.ToDictionary(k => k.Key, v => v.Value) : null,
                JogPresets = IsOverrideAllowed(definition, OverrideField.JogPresets) ? definition.MotionDefaults.JogPresets.Select(j => j.DistanceMm).ToList() : null
            }
        };

        Profiles.Add(profile);
        if (_activeProfile == null)
            _activeProfile = profile;
        SaveStore();
        ProfilesChanged?.Invoke(this, EventArgs.Empty);
        ActiveProfileChanged?.Invoke(this, EventArgs.Empty);
        return Clone(profile);
    }

    public int EnsureSystemProfilesForDefinitions(IEnumerable<MachineDefinition> definitions)
    {
        var created = 0;
        foreach (var definition in definitions)
        {
            var hasDefaultProfile = Profiles.Any(profile =>
                profile.ProfileType == RuntimeProfileType.System
                && profile.MachineDefinitionId == definition.Id
                && string.Equals(profile.MachineDefinitionVersion, definition.Version, StringComparison.OrdinalIgnoreCase));

            if (hasDefaultProfile)
                continue;

            var profile = new RuntimeProfile
            {
                ProfileName = $"{definition.DisplayNameEn} - Default",
                ProfileType = RuntimeProfileType.System,
                MachineDefinitionId = definition.Id,
                MachineDefinitionVersion = definition.Version,
                DefinitionSnapshot = Clone(definition),
                CompatibilityState = DefinitionCompatibilityState.Current,
                Overrides = new RuntimeProfileOverrides(),
                CreatedAt = DateTime.UtcNow
            };

            Profiles.Add(profile);
            if (_activeProfile == null)
                _activeProfile = profile;
            created++;
        }

        if (created > 0)
        {
            SaveStore();
            ProfilesChanged?.Invoke(this, EventArgs.Empty);
            ActiveProfileChanged?.Invoke(this, EventArgs.Empty);
        }

        return created;
    }

    public RuntimeProfile DuplicateProfile(string runtimeProfileId)
    {
        var source = Profiles.First(p => p.RuntimeProfileId == runtimeProfileId);
        if (!CanDuplicate(source))
            throw new InvalidOperationException("This runtime profile cannot be duplicated.");

        var clone = Clone(source);
        clone.RuntimeProfileId = Guid.NewGuid().ToString("N");
        clone.ProfileName = $"{source.ProfileName} Copy";
        clone.ProfileType = RuntimeProfileType.User;
        clone.IsActive = false;
        clone.CreatedAt = DateTime.UtcNow;
        clone.UpdatedAt = null;
        Profiles.Add(clone);
        SaveStore();
        ProfilesChanged?.Invoke(this, EventArgs.Empty);
        return Clone(clone);
    }

    public void SaveProfile(RuntimeProfile profile)
    {
        var existing = Profiles.FirstOrDefault(p => p.RuntimeProfileId == profile.RuntimeProfileId);
        if (existing == null)
        {
            profile.ProfileType = RuntimeProfileType.User;
            Profiles.Add(Clone(profile));
        }
        else
        {
            if (!CanEdit(existing))
                throw new InvalidOperationException("System runtime profiles are read-only. Duplicate the profile to create an editable copy.");

            var index = Profiles.IndexOf(existing);
            var updated = Clone(profile);
            updated.ProfileType = RuntimeProfileType.User;
            updated.UpdatedAt = DateTime.UtcNow;
            Profiles[index] = updated;
            if (_activeProfile?.RuntimeProfileId == updated.RuntimeProfileId)
                _activeProfile = Profiles[index];
        }

        SaveStore();
        ProfilesChanged?.Invoke(this, EventArgs.Empty);
        ActiveProfileChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool DeleteProfile(string runtimeProfileId)
    {
        var existing = Profiles.FirstOrDefault(p => p.RuntimeProfileId == runtimeProfileId);
        if (existing == null || !CanDelete(existing))
            return false;

        var wasActive = _activeProfile?.RuntimeProfileId == runtimeProfileId;
        Profiles.Remove(existing);
        if (wasActive)
            _activeProfile = Profiles.FirstOrDefault();
        SaveStore();
        ProfilesChanged?.Invoke(this, EventArgs.Empty);
        ActiveProfileChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public bool SetActiveProfile(string runtimeProfileId)
    {
        var profile = Profiles.FirstOrDefault(p => p.RuntimeProfileId == runtimeProfileId);
        if (profile == null || profile.CompatibilityState is DefinitionCompatibilityState.DefinitionIncompatible or DefinitionCompatibilityState.DefinitionMissing)
            return false;

        foreach (var p in Profiles)
            p.IsActive = p.RuntimeProfileId == runtimeProfileId;
        _activeProfile = profile;
        SaveStore();
        ProfilesChanged?.Invoke(this, EventArgs.Empty);
        ActiveProfileChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public void RecomputeCompatibility(IReadOnlyCollection<MachineDefinition> definitions)
    {
        foreach (var profile in Profiles)
        {
            var live = definitions
                .Where(d => d.Id == profile.MachineDefinitionId)
                .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
                .FirstOrDefault();

            profile.CompatibilityState = live == null && profile.DefinitionSnapshot == null
                ? DefinitionCompatibilityState.DefinitionMissing
                : live == null
                    ? DefinitionCompatibilityState.Current
                    : string.Equals(live.Version, profile.MachineDefinitionVersion, StringComparison.OrdinalIgnoreCase)
                        ? DefinitionCompatibilityState.Current
                        : DefinitionCompatibilityState.DefinitionUpdated;

            if (live != null)
                ClampOverrides(profile, live);
        }

        SaveStore();
        ProfilesChanged?.Invoke(this, EventArgs.Empty);
    }

    private void LoadStore()
    {
        try
        {
            if (File.Exists(_profilesFilePath))
            {
                var store = JsonSerializer.Deserialize<RuntimeProfileStore>(File.ReadAllText(_profilesFilePath), JsonOptions);
                if (store?.Profiles != null)
                {
                    foreach (var profile in store.Profiles)
                        Profiles.Add(profile);
                    _activeProfile = Profiles.FirstOrDefault(p => p.RuntimeProfileId == store.ActiveRuntimeProfileId) ?? Profiles.FirstOrDefault();
                }
            }
        }
        catch (JsonException)
        {
            Profiles.Clear();
            _activeProfile = null;
        }
    }

    private void SaveStore()
    {
        try
        {
            var dir = Path.GetDirectoryName(_profilesFilePath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(_profilesFilePath, JsonSerializer.Serialize(new RuntimeProfileStore
            {
                ActiveRuntimeProfileId = _activeProfile?.RuntimeProfileId,
                Profiles = Profiles.Select(Clone).ToList()
            }, JsonOptions));
        }
        catch
        {
            // Runtime profile persistence should not block active machine control.
        }
    }

    private static bool CanEdit(RuntimeProfile profile)
        => profile.ProfileType == RuntimeProfileType.User && (profile.DefinitionSnapshot?.ProfileRules.UserProfileRules.IsEditable ?? true);

    private static bool CanDelete(RuntimeProfile profile)
        => profile.ProfileType == RuntimeProfileType.User && (profile.DefinitionSnapshot?.ProfileRules.UserProfileRules.IsDeletable ?? true);

    private static bool CanDuplicate(RuntimeProfile profile)
        => profile.ProfileType == RuntimeProfileType.System
            ? profile.DefinitionSnapshot?.ProfileRules.BuiltInProfileRules.IsDuplicatable ?? true
            : profile.DefinitionSnapshot?.ProfileRules.UserProfileRules.IsDuplicatable ?? true;

    private static bool IsOverrideAllowed(MachineDefinition definition, OverrideField field)
        => definition.ProfileRules.AllowedOverrides.Contains(field);

    private static void ClampOverrides(RuntimeProfile profile, MachineDefinition definition)
    {
        foreach (var constraint in definition.ProfileRules.OverrideConstraints.Where(c => c.ConstraintType == ConstraintType.Range))
        {
            if (constraint.Field == OverrideField.BaudRate && profile.Overrides.BaudRate.HasValue)
                profile.Overrides.BaudRate = (int)Math.Clamp(profile.Overrides.BaudRate.Value, constraint.MinValue ?? 1, constraint.MaxValue ?? int.MaxValue);
        }
    }

    private static T Clone<T>(T value)
        => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, JsonOptions), JsonOptions) ?? value;
}
