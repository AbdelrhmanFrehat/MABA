using System.Windows.Input;
using MabaControlCenter.Models;
using MabaControlCenter.Services;

namespace MabaControlCenter.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    private readonly IUpdateService _updateService;
    private readonly INewsService _newsService;
    private readonly IDeviceService _deviceService;
    private readonly IModuleService _moduleService;

    private readonly ILocalizationService _localizationService;

    public DashboardViewModel(IUpdateService updateService, INewsService newsService, IDeviceService deviceService, IModuleService moduleService, ILocalizationService localizationService)
    {
        _updateService = updateService;
        _newsService = newsService;
        _deviceService = deviceService;
        _moduleService = moduleService;
        _localizationService = localizationService;
        _updateService.UpdateInfoChanged += (_, _) => OnPropertyChanged(nameof(UpdateInfo));
        _deviceService.ConnectionStateChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(ConnectedDeviceName));
            OnPropertyChanged(nameof(RecommendedModule));
        };
        _localizationService.CultureChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(PlaceholderText));
            OnPropertyChanged(nameof(ModuleCountText));
        };

        CheckForUpdatesCommand = new RelayCommand(_ => _updateService.CheckForUpdates());
    }

    public string Title => _localizationService.GetString("Page_Dashboard_Title");
    public string PlaceholderText => _localizationService.GetString("Page_Dashboard_Placeholder");

    public AppUpdateInfo UpdateInfo => _updateService.GetUpdateInfo();
    public System.Collections.ObjectModel.ObservableCollection<NewsItem> NewsItems => _newsService.NewsItems;
    public ICommand CheckForUpdatesCommand { get; }

    public string ConnectedDeviceName => _deviceService.IsConnected && _deviceService.ConnectedDeviceName != null
        ? _deviceService.ConnectedDeviceName
        : "No device connected";

    public string ConnectedDeviceHint => _deviceService.IsConnected
        ? "Live device data is available."
        : "Connect a supported device to start sending commands and calibration data.";

    public string RecommendedModule => _deviceService.IsConnected && _deviceService.ConnectedRecommendedModule != null
        ? _deviceService.ConnectedRecommendedModule
        : "No active module";

    public string RecommendedModuleHint => _deviceService.IsConnected && _deviceService.ConnectedRecommendedModule != null
        ? "Open the recommended module to access device-specific tools."
        : "No module is currently active. Open a module from the Modules section.";

    public int ModuleCount => _moduleService.AvailableModules.Count;
    public string ActiveModulesStatus => ModuleCount > 0
        ? $"{ModuleCount} module(s) available"
        : "No modules installed";

    public string ModuleCountText => string.Format(_localizationService.GetString("Format_ModulesCount"), ModuleCount);
}
