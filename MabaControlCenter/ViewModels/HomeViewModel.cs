using System.Collections.ObjectModel;
using System.Windows.Threading;
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
    private readonly DispatcherTimer _tickerTimer;
    private int _tickerIndex;
    private HomeTickerItem? _currentTickerItem;

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

        TickerItems = new ObservableCollection<HomeTickerItem>
        {
            new() { Message = "New CNC profiles available", Type = "Machine", ImageUri = "/Assets/maba.png", DisplayOrder = 10 },
            new() { Message = "SCARA module coming soon", Type = "Module", ImageUri = "/Assets/maba-logo-transparent.png", DisplayOrder = 20 },
            new() { Message = "New machine definitions synced", Type = "Catalog", ImageUri = "/Assets/maba.png", DisplayOrder = 30 },
            new() { Message = "System update deployed", Type = "System", ImageUri = "/Assets/maba-logo-transparent.png", DisplayOrder = 40 }
        };
        RefreshTickerState();

        _tickerTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _tickerTimer.Tick += (_, _) => AdvanceTicker();
        if (HasTickerItems)
        {
            _tickerTimer.Start();
        }

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
    public ObservableCollection<HomeTickerItem> TickerItems { get; }

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
    public HomeTickerItem? CurrentTickerItem
    {
        get => _currentTickerItem;
        private set
        {
            if (_currentTickerItem == value)
            {
                return;
            }

            _currentTickerItem = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentTickerMessage));
            OnPropertyChanged(nameof(CurrentTickerType));
            OnPropertyChanged(nameof(CurrentTickerImageUri));
            OnPropertyChanged(nameof(HasTickerImage));
            OnPropertyChanged(nameof(TickerPositionText));
        }
    }

    public bool HasTickerItems => GetActiveTickerItems().Count > 0;
    public string CurrentTickerMessage => CurrentTickerItem?.Message ?? string.Empty;
    public string CurrentTickerType => CurrentTickerItem?.Type ?? "Info";
    public string CurrentTickerImageUri => CurrentTickerItem?.ImageUri ?? string.Empty;
    public bool HasTickerImage => !string.IsNullOrWhiteSpace(CurrentTickerItem?.ImageUri);
    public string TickerPositionText
    {
        get
        {
            var activeItems = GetActiveTickerItems();
            if (activeItems.Count <= 1 || CurrentTickerItem == null)
            {
                return string.Empty;
            }

            var currentIndex = activeItems.FindIndex(item => item.Id == CurrentTickerItem.Id);
            return currentIndex >= 0 ? $"{currentIndex + 1}/{activeItems.Count}" : string.Empty;
        }
    }

    public string FormatModuleAction(ModuleInfo module)
        => module.IsInstalled && module.IsEnabled ? "Open" : "Coming Soon";

    public void LoadTickerItems(IEnumerable<HomeTickerItem> items)
    {
        TickerItems.Clear();
        foreach (var item in items.OrderBy(item => item.DisplayOrder))
        {
            TickerItems.Add(item);
        }

        _tickerIndex = 0;
        RefreshTickerState();

        if (HasTickerItems)
        {
            _tickerTimer.Start();
        }
        else
        {
            _tickerTimer.Stop();
        }
    }

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

    private void AdvanceTicker()
    {
        var activeItems = GetActiveTickerItems();
        if (activeItems.Count == 0)
        {
            CurrentTickerItem = null;
            _tickerTimer.Stop();
            OnPropertyChanged(nameof(HasTickerItems));
            return;
        }

        _tickerIndex = (_tickerIndex + 1) % activeItems.Count;
        CurrentTickerItem = activeItems[_tickerIndex];
        OnPropertyChanged(nameof(HasTickerItems));
    }

    private void RefreshTickerState()
    {
        var activeItems = GetActiveTickerItems();
        CurrentTickerItem = activeItems.Count > 0 ? activeItems[0] : null;
        OnPropertyChanged(nameof(HasTickerItems));
    }

    private List<HomeTickerItem> GetActiveTickerItems()
    {
        var now = DateTimeOffset.Now;
        return TickerItems
            .Where(item => item.ShouldDisplay(now))
            .OrderBy(item => item.DisplayOrder)
            .ThenBy(item => item.Message)
            .ToList();
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
