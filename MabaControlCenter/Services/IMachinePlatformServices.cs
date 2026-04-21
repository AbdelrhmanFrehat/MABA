using System.Collections.ObjectModel;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface IMachineCatalogService
{
    ObservableCollection<MachineCategory> Categories { get; }
    ObservableCollection<MachineFamily> Families { get; }
    ObservableCollection<MachineDefinitionSummary> DefinitionSummaries { get; }
    ObservableCollection<MachineDefinition> CachedDefinitions { get; }
    string LastSyncStatus { get; }
    DateTime? LastSyncedAt { get; }

    Task RefreshAsync(CancellationToken cancellationToken = default);
    MachineDefinition? GetCachedDefinition(Guid definitionId, string? version = null);
    Task<MachineDefinition?> GetDefinitionAsync(Guid definitionId, CancellationToken cancellationToken = default);
}

public interface IRuntimeProfileService
{
    ObservableCollection<RuntimeProfile> Profiles { get; }
    RuntimeProfile? ActiveProfile { get; }
    event EventHandler? ProfilesChanged;
    event EventHandler? ActiveProfileChanged;

    RuntimeProfile CreateFromDefinition(MachineDefinition definition, string profileName, RuntimeProfileType profileType, DriverType? driverOverride = null);
    int EnsureSystemProfilesForDefinitions(IEnumerable<MachineDefinition> definitions);
    RuntimeProfile DuplicateProfile(string runtimeProfileId);
    void SaveProfile(RuntimeProfile profile);
    bool DeleteProfile(string runtimeProfileId);
    bool SetActiveProfile(string runtimeProfileId);
    void RecomputeCompatibility(IReadOnlyCollection<MachineDefinition> definitions);
}

public interface IEffectiveCapabilitiesResolver
{
    CapabilitiesSection Resolve(MachineDefinition definition, CncDriverCapabilities driverCapabilities, DriverType driverType);
    CncDriverCapabilities ToDriverCapabilities(DriverType driverType);
    CncDriverType ToCncDriverType(DriverType driverType);
    DriverType FromCncDriverType(CncDriverType driverType);
}

public interface IActiveMachineContextService
{
    ActiveMachineContext Current { get; }
    event EventHandler? ContextChanged;

    ActiveMachineContext Resolve(RuntimeProfile? profile, MachineDefinition? liveDefinition = null);
}
