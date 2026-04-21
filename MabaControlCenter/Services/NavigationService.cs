using System.Collections.Generic;
using MabaControlCenter.ViewModels;

namespace MabaControlCenter.Services;

public class NavigationService : INavigationService
{
    private readonly Dictionary<string, object> _viewModels = new();
    private object? _currentViewModel;

    public NavigationService(
        IDeviceService deviceService,
        ILoggingService loggingService,
        IModuleService moduleService,
        IUpdateService updateService,
        INewsService newsService,
        IThemeService themeService,
        ILocalizationService localizationService,
        ISettingsService settingsService,
        IJobsService jobsService,
        IActiveProductionJobService activeProductionJobService,
        ICncProfileService cncProfileService,
        ICncControllerService cncControllerService,
        IGcodeParserService gcodeParserService,
        ICncExecutionQueueService cncExecutionQueueService,
        ICncJobSessionService cncJobSessionService,
        ICncPreviewPlaybackService cncPreviewPlaybackService,
        ICncFramePathService cncFramePathService,
        ICncJobPlacementService cncJobPlacementService,
        IMachineCatalogService machineCatalogService,
        IRuntimeProfileService runtimeProfileService,
        IActiveMachineContextService activeMachineContextService)
    {
        _viewModels["Dashboard"] = new DashboardViewModel(updateService, newsService, deviceService, moduleService, localizationService);
        _viewModels["Discover"] = new DiscoverViewModel(localizationService);
        _viewModels["Devices"] = new DevicesViewModel(deviceService, localizationService);
        _viewModels["Jobs"] = new JobsViewModel(jobsService, deviceService, activeProductionJobService, this);
        _viewModels["Commands"] = new CommandsViewModel(deviceService, loggingService);
        _viewModels["Modules"] = new ModulesViewModel(moduleService, deviceService, this);
        _viewModels["DexterCalibration"] = new DexterCalibrationViewModel(deviceService, loggingService, this);
        var cncControlViewModel = new CncControlViewModel(cncControllerService, cncProfileService, activeProductionJobService, this, gcodeParserService, cncExecutionQueueService, cncJobSessionService, cncPreviewPlaybackService, cncFramePathService, cncJobPlacementService, machineCatalogService, runtimeProfileService, activeMachineContextService);
        _viewModels["CncControl"] = cncControlViewModel;
        _viewModels["CncWorkspace"] = cncControlViewModel;
        _viewModels["LaserWorkspace"] = new ProductionWorkspaceViewModel("LaserWorkspace", "Laser Workspace", "LASER", activeProductionJobService, deviceService, this);
        _viewModels["Print3dWorkspace"] = new ProductionWorkspaceViewModel("Print3dWorkspace", "3D Printing Workspace", "PRINTER_3D", activeProductionJobService, deviceService, this);
        _viewModels["Logs"] = new LogsViewModel(loggingService);
        _viewModels["Settings"] = new SettingsViewModel(themeService, localizationService, settingsService);
    }

    public object? CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            if (_currentViewModel == value) return;
            _currentViewModel = value;
            CurrentViewModelChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? CurrentViewModelChanged;

    public void NavigateTo(string pageKey)
    {
        if (!_viewModels.TryGetValue(pageKey, out var vm))
            return;
        if (vm is DevicesViewModel devicesVm)
            devicesVm.LoadPorts();
        CurrentViewModel = vm;
    }
}
