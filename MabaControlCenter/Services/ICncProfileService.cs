using System.Collections.ObjectModel;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface ICncProfileService
{
    ObservableCollection<CncMachineProfile> Profiles { get; }
    CncMachineProfile ActiveProfile { get; }
    event EventHandler? ActiveProfileChanged;
    event EventHandler? ProfilesChanged;

    void SaveProfile(CncMachineProfile profile);
    CncMachineProfile CreateProfileFromMachineDefinition(MachineDefinition definition, string profileName, DriverType? driverOverride = null, RuntimeProfileType profileType = RuntimeProfileType.User);
    CncMachineProfile DuplicateProfile(string profileId);
    bool DeleteProfile(string profileId);
    void SetActiveProfile(string profileId);
    void RestoreDefaultProfiles();
}
