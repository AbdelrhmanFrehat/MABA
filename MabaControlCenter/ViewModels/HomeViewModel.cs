using System.Collections.ObjectModel;
using System.Windows.Input;
using MabaControlCenter.Models;
using MabaControlCenter.Services;

namespace MabaControlCenter.ViewModels;

public class HomeViewModel : ViewModelBase
{
    private readonly IAuthSessionService _authSessionService;
    private readonly IDeviceService _deviceService;
    private readonly IModuleService _moduleService;
    private readonly IActiveMachineContextService _activeMachineContextService;
    private readonly INavigationService _navigationService;

    public HomeViewModel(
        IAuthSessionService authSessionService,
        IDeviceService deviceService,
        IModuleService moduleService,
        IActiveMachineContextService activeMachineContextService,
        INavigationService navigationService)
    {
        _authSessionService = authSessionService;
        _deviceService = deviceService;
        _moduleService = moduleService;
        _activeMachineContextService = activeMachineContextService;
        _navigationService = navigationService;

        OpenControlPanelCommand = new RelayCommand(_ => _navigationService.NavigateTo("CncControl"));
        AddMachineCommand = new RelayCommand(_ => _navigationService.NavigateTo("CncControl"));
        OpenDevicesCommand = new RelayCommand(_ => _navigationService.NavigateTo("Devices"));
        OpenJobsCommand = new RelayCommand(_ => _navigationService.NavigateTo("Jobs"));
        OpenModulesCommand = new RelayCommand(_ => _navigationService.NavigateTo("Modules"));

        QuickAccessCards = new ObservableCollection<HomeActionCard>
        {
            new() { Icon = "\uE8A7", Title = "Control Panel", Description = "Open the CNC and automation operator workspace.", Command = OpenControlPanelCommand },
            new() { Icon = "\uE772", Title = "Devices", Description = "Connect, discover, and inspect local machines.", Command = OpenDevicesCommand },
            new() { Icon = "\uE9D2", Title = "Jobs", Description = "Review production jobs from the MABA backend.", Command = OpenJobsCommand },
            new() { Icon = "\uE8B7", Title = "Modules", Description = "Open installed machine modules and tools.", Command = OpenModulesCommand }
        };

        WhatIsNew = new ObservableCollection<HomeUpdateItem>
        {
            new() { Title = "Machine platform", Description = "Runtime profiles, drivers, and machine definitions now power CNC setup." },
            new() { Title = "CNC workspace", Description = "Preview, placement, simulation, and visualization are now grouped for operators." },
            new() { Title = "Production jobs", Description = "Approved backend jobs can now appear in the Control Center job flow." }
        };

        ModuleRows = new ObservableCollection<HomeModuleRow>();
        RefreshModuleRows();

        _authSessionService.AuthenticationChanged += (_, _) => OnPropertyChanged(nameof(UserName));
        _deviceService.ConnectionStateChanged += (_, _) => RefreshMachineState();
        _activeMachineContextService.ContextChanged += (_, _) => RefreshMachineState();
    }

    public ObservableCollection<HomeActionCard> QuickAccessCards { get; }
    public ObservableCollection<ModuleInfo> Modules => _moduleService.AvailableModules;
    public ObservableCollection<HomeModuleRow> ModuleRows { get; }
    public ObservableCollection<HomeUpdateItem> WhatIsNew { get; }

    public ICommand OpenControlPanelCommand { get; }
    public ICommand AddMachineCommand { get; }
    public ICommand OpenDevicesCommand { get; }
    public ICommand OpenJobsCommand { get; }
    public ICommand OpenModulesCommand { get; }

    public string UserName => _authSessionService.CurrentUser?.FullName ?? _authSessionService.CurrentUser?.Email ?? "MABA user";
    public bool HasActiveMachine => _activeMachineContextService.Current.MachineDefinition != null || _activeMachineContextService.Current.RuntimeProfile != null;
    public string ActiveMachineName => _activeMachineContextService.Current.MachineDefinition?.DisplayNameEn
        ?? _activeMachineContextService.Current.RuntimeProfile?.ProfileName
        ?? "No machine configured";
    public string ActiveMachineStatus => _deviceService.IsConnected ? "Connected" : "Idle";
    public string ActiveMachineMode => _activeMachineContextService.Current.DriverType == DriverType.Simulated
        ? "Simulation"
        : _activeMachineContextService.Current.DriverType.ToString();
    public string ActiveMachineSummary => HasActiveMachine
        ? $"{ActiveMachineName} is ready for setup or operation."
        : "No machine configured";

    public string FormatModuleAction(ModuleInfo module)
        => module.IsInstalled && module.IsEnabled ? "Open" : "Coming Soon";

    private void RefreshModuleRows()
    {
        ModuleRows.Clear();

        foreach (var module in _moduleService.AvailableModules)
        {
            var isAvailable = module.IsInstalled && module.IsEnabled;
            ModuleRows.Add(new HomeModuleRow
            {
                Name = module.Name,
                Description = module.Description,
                ActionText = isAvailable ? "Open" : "Coming Soon",
                IsAvailable = isAvailable,
                Command = isAvailable ? OpenModulesCommand : null
            });
        }
    }

    private void RefreshMachineState()
    {
        OnPropertyChanged(nameof(HasActiveMachine));
        OnPropertyChanged(nameof(ActiveMachineName));
        OnPropertyChanged(nameof(ActiveMachineStatus));
        OnPropertyChanged(nameof(ActiveMachineMode));
        OnPropertyChanged(nameof(ActiveMachineSummary));
    }
}
