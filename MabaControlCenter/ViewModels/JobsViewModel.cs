using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using MabaControlCenter.Models;
using MabaControlCenter.Services;

namespace MabaControlCenter.ViewModels;

public class JobsViewModel : ViewModelBase
{
    private readonly IJobsService _jobsService;
    private readonly IDeviceService _deviceService;
    private bool _isLoading;
    private string? _errorMessage;
    private const string AllFilter = "All";
    private string _selectedStatusFilter = AllFilter;
    private string _selectedMachineFilter = AllFilter;
    private ControlCenterJobListItem? _selectedJob;
    private ControlCenterJobDetail? _selectedJobDetail;

    public JobsViewModel(IJobsService jobsService, IDeviceService deviceService)
    {
        _jobsService = jobsService;
        _deviceService = deviceService;
        _deviceService.ConnectionStateChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CompatibilityText));
            OnPropertyChanged(nameof(ActiveDeviceSummary));
        };

        Jobs = new ObservableCollection<ControlCenterJobListItem>();
        Attachments = new ObservableCollection<ControlCenterJobAttachment>();
        PayloadEntries = new ObservableCollection<JobPayloadEntry>();
        StatusFilters = new ObservableCollection<string>(new[] { AllFilter, "Pending", "Ready", "InProgress", "Completed", "Failed", "Cancelled" });
        MachineFilters = new ObservableCollection<string>(new[] { AllFilter, "CNC", "PRINTER_3D", "LASER", "DEXTER", "SCARA" });
        _selectedStatusFilter = StatusFilters[0];
        _selectedMachineFilter = MachineFilters[0];

        RefreshCommand = new RelayCommand(_ => _ = LoadJobsAsync(), _ => !IsLoading);
        ApproveCommand = new RelayCommand(_ => _ = RunJobActionAsync("approve"), _ => !IsLoading && CanApprove);
        StartCommand = new RelayCommand(_ => _ = RunJobActionAsync("start"), _ => !IsLoading && CanStart);
        CompleteCommand = new RelayCommand(_ => _ = RunJobActionAsync("complete"), _ => !IsLoading && CanComplete);
        FailCommand = new RelayCommand(_ => _ = RunJobActionAsync("fail"), _ => !IsLoading && CanFail);
        CancelCommand = new RelayCommand(_ => _ = RunJobActionAsync("cancel"), _ => !IsLoading && CanCancel);
        LoadInitialData();
    }

    public ObservableCollection<ControlCenterJobListItem> Jobs { get; }
    public ObservableCollection<ControlCenterJobAttachment> Attachments { get; }
    public ObservableCollection<JobPayloadEntry> PayloadEntries { get; }
    public ObservableCollection<string> StatusFilters { get; }
    public ObservableCollection<string> MachineFilters { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ApproveCommand { get; }
    public ICommand StartCommand { get; }
    public ICommand CompleteCommand { get; }
    public ICommand FailCommand { get; }
    public ICommand CancelCommand { get; }

    public string Title => "Jobs";
    public string Subtitle => "Production jobs bridged from the MABA backend for operator visibility.";
    public bool HasJobs => Jobs.Count > 0;
    public string EmptyStateTitle => "No production jobs found.";
    public string EmptyStateHint => "Try changing filters or create/approve a service request first.";

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (_isLoading == value) return;
            _isLoading = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (_errorMessage == value) return;
            _errorMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public string SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set
        {
            if (_selectedStatusFilter == value) return;
            _selectedStatusFilter = value;
            OnPropertyChanged();
            _ = LoadJobsAsync();
        }
    }

    public string SelectedMachineFilter
    {
        get => _selectedMachineFilter;
        set
        {
            if (_selectedMachineFilter == value) return;
            _selectedMachineFilter = value;
            OnPropertyChanged();
            _ = LoadJobsAsync();
        }
    }

    public ControlCenterJobListItem? SelectedJob
    {
        get => _selectedJob;
        set
        {
            if (_selectedJob == value) return;
            _selectedJob = value;
            OnPropertyChanged();
            _ = SelectJobAsync(value);
        }
    }

    public ControlCenterJobDetail? SelectedJobDetail
    {
        get => _selectedJobDetail;
        private set
        {
            if (_selectedJobDetail == value) return;
            _selectedJobDetail = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedJobDescription));
            OnPropertyChanged(nameof(SourceLabel));
            OnPropertyChanged(nameof(MachineLabel));
            OnPropertyChanged(nameof(AssignedDeviceLabel));
            OnPropertyChanged(nameof(StartedAtLabel));
            OnPropertyChanged(nameof(CompletedAtLabel));
            OnPropertyChanged(nameof(CompatibilityText));
            OnPropertyChanged(nameof(CanApprove));
            OnPropertyChanged(nameof(CanStart));
            OnPropertyChanged(nameof(CanComplete));
            OnPropertyChanged(nameof(CanFail));
            OnPropertyChanged(nameof(CanCancel));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string SelectedJobDescription => SelectedJobDetail?.Description ?? "No description available.";
    public string SourceLabel => SelectedJobDetail == null ? "N/A" : MapSourceLabel(SelectedJobDetail.SourceType);
    public string MachineLabel => SelectedJobDetail == null ? "Unknown" : MapMachineLabel(SelectedJobDetail.MachineType);
    public string AssignedDeviceLabel => SelectedJobDetail?.AssignedDeviceId?.ToString() ?? "Unassigned";
    public string StartedAtLabel => SelectedJobDetail?.StartedAt?.ToLocalTime().ToString("g") ?? "Not started";
    public string CompletedAtLabel => SelectedJobDetail?.CompletedAt?.ToLocalTime().ToString("g") ?? "Not completed";
    public bool CanApprove => SelectedJobDetail?.Status == "Pending";
    public bool CanStart => SelectedJobDetail?.Status == "Ready";
    public bool CanComplete => SelectedJobDetail?.Status == "InProgress";
    public bool CanFail => SelectedJobDetail?.Status == "InProgress";
    public bool CanCancel => SelectedJobDetail?.Status is "Pending" or "Ready" or "InProgress";

    public string ActiveDeviceSummary
    {
        get
        {
            if (!_deviceService.IsConnected)
            {
                return "No active device connected in Control Center.";
            }

            return $"{_deviceService.ConnectedDeviceName ?? "Connected device"} ({_deviceService.ConnectedProductCode ?? "Unknown code"})";
        }
    }

    public string CompatibilityText
    {
        get
        {
            if (SelectedJobDetail == null)
            {
                return "Select a job to see device compatibility.";
            }

            var requiredDevice = MapRequiredDevice(SelectedJobDetail.MachineType);
            if (requiredDevice == null)
            {
                return "This job type has no simulated device mapping in the current WPF app.";
            }

            if (!_deviceService.IsConnected)
            {
                return $"Requires a compatible {requiredDevice} device.";
            }

            var connectedCode = _deviceService.ConnectedProductCode ?? string.Empty;
            if (connectedCode.Contains(requiredDevice, StringComparison.OrdinalIgnoreCase))
            {
                return $"Connected device {_deviceService.ConnectedDeviceName} is compatible with this job.";
            }

            return $"Connected device {_deviceService.ConnectedDeviceName ?? "Unknown"} does not match required machine type {MapMachineLabel(SelectedJobDetail.MachineType)}.";
        }
    }

    private async void LoadInitialData()
    {
        await LoadJobsAsync();
    }

    private async Task LoadJobsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            Jobs.Clear();

            var jobs = await _jobsService.GetJobsAsync(
                string.IsNullOrWhiteSpace(SelectedStatusFilter) || SelectedStatusFilter == AllFilter ? null : SelectedStatusFilter,
                string.IsNullOrWhiteSpace(SelectedMachineFilter) || SelectedMachineFilter == AllFilter ? null : SelectedMachineFilter);

            foreach (var job in jobs)
            {
                Jobs.Add(job);
            }

            if (Jobs.Count > 0)
            {
                if (SelectedJob == null || Jobs.All(x => x.Id != SelectedJob.Id))
                {
                    SelectedJob = Jobs[0];
                }
            }
            else
            {
                SelectedJob = null;
                SelectedJobDetail = null;
                Attachments.Clear();
                PayloadEntries.Clear();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(HasJobs));
        }
    }

    private async Task SelectJobAsync(ControlCenterJobListItem? job)
    {
        if (job == null)
        {
            SelectedJobDetail = null;
            Attachments.Clear();
            PayloadEntries.Clear();
            return;
        }

        try
        {
            ErrorMessage = null;
            var detail = await _jobsService.GetJobAsync(job.Id);
            SelectedJobDetail = detail;

            Attachments.Clear();
            PayloadEntries.Clear();

            if (detail == null)
            {
                return;
            }

            foreach (var attachment in detail.Attachments)
            {
                Attachments.Add(attachment);
            }

            foreach (var entry in ParsePayload(detail.PayloadJson))
            {
                PayloadEntries.Add(entry);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private async Task RunJobActionAsync(string action)
    {
        if (SelectedJobDetail == null)
        {
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var updated = await _jobsService.RunActionAsync(SelectedJobDetail.Id, action);
            SelectedJobDetail = updated;

            await LoadJobsAsync();

            if (updated != null)
            {
                SelectedJob = Jobs.FirstOrDefault(x => x.Id == updated.Id);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static IEnumerable<JobPayloadEntry> ParsePayload(string? payloadJson)
    {
        var entries = new List<JobPayloadEntry>();

        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            return entries;
        }

        try
        {
            using var document = JsonDocument.Parse(payloadJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                entries.Add(new JobPayloadEntry { Key = "payload", Value = payloadJson });
                return entries;
            }

            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Null)
                {
                    continue;
                }

                var value = property.Value.ValueKind switch
                {
                    JsonValueKind.Object or JsonValueKind.Array => property.Value.GetRawText(),
                    JsonValueKind.String => property.Value.GetString() ?? string.Empty,
                    _ => property.Value.ToString()
                };

                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                entries.Add(new JobPayloadEntry { Key = property.Name, Value = value });
            }
        }
        catch
        {
            entries.Add(new JobPayloadEntry { Key = "payload", Value = payloadJson });
        }

        return entries;
    }

    private static string MapSourceLabel(string sourceType) => sourceType switch
    {
        "PRINT_REQUEST" => "3D Print Request",
        "CNC_REQUEST" => "CNC Request",
        "LASER_REQUEST" => "Laser Request",
        "ORDER" => "Order",
        _ => sourceType
    };

    private static string MapMachineLabel(string? machineType) => machineType switch
    {
        "PRINTER_3D" => "3D Printer",
        null or "" => "Unknown",
        _ => machineType
    };

    private static string? MapRequiredDevice(string? machineType) => machineType switch
    {
        "CNC" => "CNC",
        "DEXTER" => "DEXTER",
        "SCARA" => "SCARA",
        "LASER" => "LASER",
        "PRINTER_3D" => "PRINTER_3D",
        _ => null
    };
}
