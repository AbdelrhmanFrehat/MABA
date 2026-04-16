using System.Collections.ObjectModel;
using System.Windows.Input;
using MabaControlCenter.Models;
using MabaControlCenter.Services;

namespace MabaControlCenter.ViewModels;

public class ProductionWorkspaceViewModel : ViewModelBase
{
    private readonly string _workspaceKey;
    private readonly string _workspaceTitle;
    private readonly string _expectedMachineType;
    private readonly IActiveProductionJobService _activeJobService;
    private readonly IDeviceService _deviceService;
    private readonly INavigationService _navigationService;
    private ActiveProductionJobContext? _activeJob;

    public ProductionWorkspaceViewModel(
        string workspaceKey,
        string workspaceTitle,
        string expectedMachineType,
        IActiveProductionJobService activeJobService,
        IDeviceService deviceService,
        INavigationService navigationService)
    {
        _workspaceKey = workspaceKey;
        _workspaceTitle = workspaceTitle;
        _expectedMachineType = expectedMachineType;
        _activeJobService = activeJobService;
        _deviceService = deviceService;
        _navigationService = navigationService;

        Attachments = new ObservableCollection<ControlCenterJobAttachment>();

        _activeJobService.CurrentJobChanged += (_, _) => RefreshJobContext();
        _deviceService.ConnectionStateChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(DeviceStatusText));
            OnPropertyChanged(nameof(DeviceCompatibilityText));
        };

        BackToJobsCommand = new RelayCommand(_ => _navigationService.NavigateTo("Jobs"));
        RefreshJobContext();
    }

    public string WorkspaceKey => _workspaceKey;
    public string Title => _workspaceTitle;
    public string Subtitle => "Operator workspace opened from a real production job.";
    public ICommand BackToJobsCommand { get; }
    public ObservableCollection<ControlCenterJobAttachment> Attachments { get; }
    public bool HasAttachments => Attachments.Count > 0;

    public ActiveProductionJobContext? ActiveJob
    {
        get => _activeJob;
        private set
        {
            if (_activeJob == value) return;
            _activeJob = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasActiveJob));
            OnPropertyChanged(nameof(JobTitle));
            OnPropertyChanged(nameof(JobReference));
            OnPropertyChanged(nameof(SourceReference));
            OnPropertyChanged(nameof(MachineType));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(StatusDisplay));
            OnPropertyChanged(nameof(SourceTypeDisplay));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(CustomerName));
            OnPropertyChanged(nameof(DeviceCompatibilityText));
        }
    }

    public bool HasActiveJob => ActiveJob != null;
    public string JobTitle => ActiveJob?.Title ?? "No active job";
    public string JobReference => ActiveJob?.JobReference ?? "No job selected";
    public string SourceReference => ActiveJob?.SourceReference ?? "N/A";
    public string MachineType => ActiveJob?.MachineTypeDisplay ?? ControlCenterJobDisplay.FormatMachineType(_expectedMachineType);
    public string Status => ActiveJob?.Status ?? "Unknown";
    public string StatusDisplay => ActiveJob?.StatusDisplay ?? "Unknown";
    public string SourceTypeDisplay => ActiveJob?.SourceTypeDisplay ?? "Unknown";
    public string Description => ActiveJob?.Description ?? "No job context has been handed off to this workspace yet.";
    public string CustomerName => ActiveJob?.CustomerName ?? "N/A";

    public string DeviceStatusText => !_deviceService.IsConnected
        ? "No active device connected."
        : $"{_deviceService.ConnectedDeviceName ?? "Connected device"} ({_deviceService.ConnectedProductCode ?? "Unknown code"})";

    public string DeviceCompatibilityText
    {
        get
        {
            if (ActiveJob == null)
            {
                return "Open a job from the Jobs page to load module execution context.";
            }

            if (!_deviceService.IsConnected)
            {
                return $"Connect a compatible {MachineType} device to continue execution.";
            }

            var connectedCode = _deviceService.ConnectedProductCode ?? string.Empty;
            var requiredCode = _expectedMachineType switch
            {
                "CNC" => "CNC",
                "LASER" => "LASER",
                "PRINTER_3D" => "PRINTER_3D",
                _ => _expectedMachineType
            };

            return connectedCode.Contains(requiredCode, StringComparison.OrdinalIgnoreCase)
                ? $"Connected device {_deviceService.ConnectedDeviceName} is compatible with this job."
                : $"Connected device {_deviceService.ConnectedDeviceName ?? "Unknown"} does not match required machine type {MachineType}.";
        }
    }

    private void RefreshJobContext()
    {
        var job = _activeJobService.CurrentJob;
        if (job?.MachineType != _expectedMachineType)
        {
            ActiveJob = null;
            Attachments.Clear();
            OnPropertyChanged(nameof(HasAttachments));
            return;
        }

        ActiveJob = job;
        Attachments.Clear();
        OnPropertyChanged(nameof(HasAttachments));

        if (job == null)
        {
            return;
        }

        foreach (var attachment in job.Attachments)
        {
            Attachments.Add(attachment);
        }

        OnPropertyChanged(nameof(HasAttachments));
    }
}
