using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using MabaControlCenter.Models;
using MabaControlCenter.Services;

namespace MabaControlCenter.ViewModels;

public class DevicesViewModel : ViewModelBase
{
    private const string SoftwareUrl = "https://mabasol.com/software";
    private const string WebsiteUrl = "https://mabasol.com/";

    private readonly IDeviceService _deviceService;
    private readonly ILocalizationService _localizationService;
    private string? _selectedPort;
    private DeviceProfile? _deviceProfile;

    public DevicesViewModel(IDeviceService deviceService, ILocalizationService localizationService)
    {
        _deviceService = deviceService;
        _localizationService = localizationService;
        _deviceService.ConnectionStateChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(ConnectionStatus));
            CommandManager.InvalidateRequerySuggested();
        };
        _localizationService.CultureChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(PlaceholderText));
            OnPropertyChanged(nameof(ConnectionStatus));
        };

        Ports = new ObservableCollection<string>();
        LoadPorts();

        ConnectCommand = new RelayCommand(_ => Connect(), _ => CanConnect());
        DisconnectCommand = new RelayCommand(_ => Disconnect(), _ => CanDisconnect());
        RefreshPortsCommand = new RelayCommand(_ => LoadPorts());
        OpenUrlCommand = new RelayCommand(OpenUrl);
    }

    private static void OpenUrl(object? parameter)
    {
        if (parameter is not string url || string.IsNullOrWhiteSpace(url)) return;
        try
        {
            var startInfo = new ProcessStartInfo { FileName = url, UseShellExecute = true };
            Process.Start(startInfo);
        }
        catch { /* ignore */ }
    }

    public string Title => _localizationService.GetString("Page_Devices_Title");
    public string PlaceholderText => _localizationService.GetString("Page_Devices_Placeholder");

    public ObservableCollection<string> Ports { get; }

    public DeviceProfile? DeviceProfile
    {
        get => _deviceProfile;
        private set
        {
            if (_deviceProfile == value) return;
            _deviceProfile = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowNoDevicePanel));
            OnPropertyChanged(nameof(ShowUnsupportedPanel));
            OnPropertyChanged(nameof(ShowSupportedPanel));
        }
    }

    public bool ShowNoDevicePanel => DeviceProfile == null;
    public bool ShowUnsupportedPanel => DeviceProfile != null && !DeviceProfile.IsSupported;
    public bool ShowSupportedPanel => DeviceProfile != null && DeviceProfile.IsSupported;

    public string SoftwareUrlValue => SoftwareUrl;
    public string WebsiteUrlValue => WebsiteUrl;

    public string? SelectedPort
    {
        get => _selectedPort;
        set
        {
            if (_selectedPort == value) return;
            _selectedPort = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string ConnectionStatus => _deviceService.IsConnected ? _localizationService.GetString("Status_Connected") : _localizationService.GetString("Status_Disconnected");

    public ICommand ConnectCommand { get; }
    public ICommand DisconnectCommand { get; }
    public ICommand RefreshPortsCommand { get; }
    public ICommand OpenUrlCommand { get; }

    /// <summary>
    /// Loads COM ports from DeviceService into Ports. Call when the page loads so the list is fresh.
    /// </summary>
    public void LoadPorts()
    {
        Ports.Clear();
        foreach (var port in _deviceService.GetAvailablePorts())
            Ports.Add(port);
    }

    private bool CanConnect() =>
        !_deviceService.IsConnected &&
        (_deviceService.IsSimulationMode || !string.IsNullOrWhiteSpace(SelectedPort));

    private bool CanDisconnect() => _deviceService.IsConnected;

    private void Connect()
    {
        if (!_deviceService.IsSimulationMode && string.IsNullOrWhiteSpace(SelectedPort)) return;
        _deviceService.Connect(SelectedPort ?? "");
        DeviceProfile = _deviceService.PerformHandshake();
    }

    private void Disconnect()
    {
        _deviceService.Disconnect();
        DeviceProfile = null;
    }
}
