using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MabaControlCenter.Services;

namespace MabaControlCenter.ViewModels;

public class ModulesViewModel : ViewModelBase
{
    private readonly IModuleService _moduleService;
    private readonly IDeviceService _deviceService;
    private readonly INavigationService _navigationService;

    public ModulesViewModel(IModuleService moduleService, IDeviceService deviceService, INavigationService navigationService)
    {
        _moduleService = moduleService;
        _deviceService = deviceService;
        _navigationService = navigationService;
        _deviceService.ConnectionStateChanged += (_, _) => RefreshRecommendedStatus();

        Modules = new ObservableCollection<ModuleDisplayItem>();
        foreach (var m in _moduleService.AvailableModules)
            Modules.Add(new ModuleDisplayItem(m));
        RefreshRecommendedStatus();

        OpenModuleCommand = new RelayCommand(OpenModule);
    }

    public string Title => "Modules";
    public string PlaceholderText => "Available modules for MABA devices.";

    public ObservableCollection<ModuleDisplayItem> Modules { get; }
    public ICommand OpenModuleCommand { get; }

    private void OpenModule(object? parameter)
    {
        if (parameter is not ModuleDisplayItem item) return;
        var m = item.Module;

        if (!m.IsInstalled)
        {
            MessageBox.Show("Module not installed.", "Modules", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (!m.IsEnabled)
        {
            MessageBox.Show("Module is disabled.", "Modules", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (m.Code == "DEXTER-CAL")
            _navigationService.NavigateTo("DexterCalibration");
        else
            MessageBox.Show("Module page not available yet.", "Modules", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void RefreshRecommendedStatus()
    {
        var code = _deviceService.ConnectedProductCode;
        foreach (var item in Modules)
            item.IsRecommended = code != null && item.Module.SupportedProductCodes.Contains(code);
    }
}
