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
    private readonly IActiveProductionJobService _activeProductionJobService;
    private readonly INavigationService _navigationService;
    private bool _isLoading;
    private bool _isExecuting;
    private string? _errorMessage;
    private const string AllFilter = "All";
    private JobFilterOption? _selectedStatusFilter;
    private JobFilterOption? _selectedMachineFilter;
    private ControlCenterJobListItem? _selectedJob;
    private ControlCenterJobDetail? _selectedJobDetail;

    public JobsViewModel(
        IJobsService jobsService,
        IDeviceService deviceService,
        IActiveProductionJobService activeProductionJobService,
        INavigationService navigationService)
    {
        _jobsService = jobsService;
        _deviceService = deviceService;
        _activeProductionJobService = activeProductionJobService;
        _navigationService = navigationService;
        _deviceService.ConnectionStateChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CompatibilityText));
            OnPropertyChanged(nameof(ActiveDeviceSummary));
            OnPropertyChanged(nameof(CanStart));
            OnPropertyChanged(nameof(ShowStartBlockedMessage));
        };

        Jobs = new ObservableCollection<ControlCenterJobListItem>();
        Attachments = new ObservableCollection<ControlCenterJobAttachment>();
        PayloadEntries = new ObservableCollection<JobPayloadEntry>();
        StatusFilters = new ObservableCollection<JobFilterOption>(new[]
        {
            new JobFilterOption { Value = AllFilter, Label = AllFilter },
            new JobFilterOption { Value = "Pending", Label = "Pending" },
            new JobFilterOption { Value = "Ready", Label = "Ready" },
            new JobFilterOption { Value = "InProgress", Label = "In Progress" },
            new JobFilterOption { Value = "Completed", Label = "Completed" },
            new JobFilterOption { Value = "Failed", Label = "Failed" },
            new JobFilterOption { Value = "Cancelled", Label = "Cancelled" }
        });
        MachineFilters = new ObservableCollection<JobFilterOption>(new[]
        {
            new JobFilterOption { Value = AllFilter, Label = AllFilter },
            new JobFilterOption { Value = "CNC", Label = "CNC" },
            new JobFilterOption { Value = "PRINTER_3D", Label = "3D Printer" },
            new JobFilterOption { Value = "LASER", Label = "Laser" },
            new JobFilterOption { Value = "DEXTER", Label = "Dexter" },
            new JobFilterOption { Value = "SCARA", Label = "SCARA" }
        });
        _selectedStatusFilter = StatusFilters[0];
        _selectedMachineFilter = MachineFilters[0];

        RefreshCommand = new RelayCommand(_ => _ = LoadJobsAsync(), _ => !IsLoading);
        ApproveCommand = new RelayCommand(_ => _ = MarkReadyAsync(), _ => CanApprove);
        StartCommand = new RelayCommand(_ => _ = StartJobExecutionAsync(), _ => CanStart);
        CompleteCommand = new RelayCommand(_ => _ = CompleteJobAsync(), _ => CanComplete);
        FailCommand = new RelayCommand(_ => _ = FailJobAsync(), _ => CanFail);
        CancelCommand = new RelayCommand(_ => _ = CancelJobAsync(), _ => CanCancel);
        OpenInModuleCommand = new RelayCommand(_ => OpenInModule(), _ => CanOpenInModule);
        LoadInitialData();
    }

    public ObservableCollection<ControlCenterJobListItem> Jobs { get; }
    public ObservableCollection<ControlCenterJobAttachment> Attachments { get; }
    public ObservableCollection<JobPayloadEntry> PayloadEntries { get; }
    public ObservableCollection<JobFilterOption> StatusFilters { get; }
    public ObservableCollection<JobFilterOption> MachineFilters { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ApproveCommand { get; }
    public ICommand StartCommand { get; }
    public ICommand CompleteCommand { get; }
    public ICommand FailCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand OpenInModuleCommand { get; }

    public string Title => "Jobs";
    public string Subtitle => "Production jobs bridged from the MABA backend for operator visibility.";
    public string StatusFilterLabel => "Filter by Status";
    public string MachineFilterLabel => "Filter by Machine Type";
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
            OnPropertyChanged(nameof(IsBusy));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public bool IsExecuting
    {
        get => _isExecuting;
        private set
        {
            if (_isExecuting == value) return;
            _isExecuting = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(ExecutionStatusText));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public bool IsBusy => IsLoading || IsExecuting;

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

    public JobFilterOption? SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set
        {
            if (ReferenceEquals(_selectedStatusFilter, value)) return;
            _selectedStatusFilter = value;
            OnPropertyChanged();
            _ = LoadJobsAsync();
        }
    }

    public JobFilterOption? SelectedMachineFilter
    {
        get => _selectedMachineFilter;
        set
        {
            if (ReferenceEquals(_selectedMachineFilter, value)) return;
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
            OnPropertyChanged(nameof(StatusLabel));
            OnPropertyChanged(nameof(AssignedDeviceLabel));
            OnPropertyChanged(nameof(StartedAtLabel));
            OnPropertyChanged(nameof(CompletedAtLabel));
            OnPropertyChanged(nameof(CompatibilityText));
            OnPropertyChanged(nameof(CanApprove));
            OnPropertyChanged(nameof(CanStart));
            OnPropertyChanged(nameof(CanComplete));
            OnPropertyChanged(nameof(CanFail));
            OnPropertyChanged(nameof(CanCancel));
            OnPropertyChanged(nameof(ShowApproveAction));
            OnPropertyChanged(nameof(ShowStartAction));
            OnPropertyChanged(nameof(ShowCompleteAction));
            OnPropertyChanged(nameof(ShowFailAction));
            OnPropertyChanged(nameof(ShowCancelAction));
            OnPropertyChanged(nameof(ShowStartBlockedMessage));
            OnPropertyChanged(nameof(CanOpenInModule));
            OnPropertyChanged(nameof(OpenInModuleLabel));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string SelectedJobDescription => SelectedJobDetail?.Description ?? "No description available.";
    public string SourceLabel => SelectedJobDetail == null ? "N/A" : MapSourceLabel(SelectedJobDetail.SourceType);
    public string MachineLabel => SelectedJobDetail == null ? "Unknown" : MapMachineLabel(SelectedJobDetail.MachineType);
    public string StatusLabel => SelectedJobDetail == null ? "Unknown" : ControlCenterJobDisplay.FormatStatus(SelectedJobDetail.Status);
    public string AssignedDeviceLabel => SelectedJobDetail?.AssignedDeviceId?.ToString() ?? "Unassigned";
    public string StartedAtLabel => SelectedJobDetail?.StartedAt?.ToLocalTime().ToString("g") ?? "Not started";
    public string CompletedAtLabel => SelectedJobDetail?.CompletedAt?.ToLocalTime().ToString("g") ?? "Not completed";
    public bool ShowApproveAction => SelectedJobDetail?.Status == "Pending";
    public bool ShowStartAction => SelectedJobDetail?.Status == "Ready";
    public bool ShowCompleteAction => SelectedJobDetail?.Status == "InProgress";
    public bool ShowFailAction => SelectedJobDetail?.Status == "InProgress";
    public bool ShowCancelAction => SelectedJobDetail?.Status != null && SelectedJobDetail.Status != "Cancelled";
    public bool CanApprove => ShowApproveAction && !IsBusy;
    public bool CanStart => ShowStartAction && _deviceService.IsConnected && !IsBusy;
    public bool CanComplete => ShowCompleteAction && !IsBusy;
    public bool CanFail => ShowFailAction && !IsBusy;
    public bool CanCancel => ShowCancelAction && !IsBusy;
    public bool ShowStartBlockedMessage => ShowStartAction && !_deviceService.IsConnected;
    public string ExecutionStatusText => IsExecuting ? "Executing..." : string.Empty;
    public bool CanOpenInModule => SelectedJobDetail is { } detail
        && detail.MachineType is "CNC" or "LASER" or "PRINTER_3D"
        && detail.Status is "Ready" or "InProgress"
        && !IsBusy;
    public string OpenInModuleLabel => SelectedJobDetail?.MachineType switch
    {
        "CNC" => "Open in CNC Workspace",
        "LASER" => "Open in Laser Workspace",
        "PRINTER_3D" => "Open in 3D Printing Workspace",
        _ => "Open in Module"
    };

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
                string.IsNullOrWhiteSpace(SelectedStatusFilter?.Value) || SelectedStatusFilter.Value == AllFilter ? null : SelectedStatusFilter.Value,
                string.IsNullOrWhiteSpace(SelectedMachineFilter?.Value) || SelectedMachineFilter.Value == AllFilter ? null : SelectedMachineFilter.Value);

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

    private async Task MarkReadyAsync() => await ExecuteSingleActionAsync((id, ct) => _jobsService.MarkReadyAsync(id, ct));

    private async Task CompleteJobAsync() => await ExecuteSingleActionAsync((id, ct) => _jobsService.CompleteJobAsync(id, ct));

    private async Task FailJobAsync() => await ExecuteSingleActionAsync((id, ct) => _jobsService.FailJobAsync(id, ct));

    private async Task CancelJobAsync() => await ExecuteSingleActionAsync((id, ct) => _jobsService.CancelJobAsync(id, ct));

    private async Task StartJobExecutionAsync()
    {
        if (SelectedJobDetail == null)
        {
            return;
        }

        if (!_deviceService.IsConnected)
        {
            ErrorMessage = "Connect a device to start this job";
            return;
        }

        try
        {
            ErrorMessage = null;
            IsLoading = true;

            var started = await _jobsService.StartJobAsync(SelectedJobDetail.Id);
            await RefreshSelectedJobAsync(started);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return;
        }
        finally
        {
            IsLoading = false;
        }

        try
        {
            IsExecuting = true;
            await Task.Delay(TimeSpan.FromSeconds(4));

            var currentId = SelectedJobDetail?.Id;
            if (currentId.HasValue)
            {
                var completed = await _jobsService.CompleteJobAsync(currentId.Value);
                await RefreshSelectedJobAsync(completed);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            await LoadJobsAsync();
        }
        finally
        {
            IsExecuting = false;
        }
    }

    private async Task ExecuteSingleActionAsync(Func<Guid, CancellationToken, Task<ControlCenterJobDetail?>> action)
    {
        if (SelectedJobDetail == null)
        {
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var updated = await action(SelectedJobDetail.Id, CancellationToken.None);
            await RefreshSelectedJobAsync(updated);
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

    private async Task RefreshSelectedJobAsync(ControlCenterJobDetail? updated)
    {
        SelectedJobDetail = updated;
        await LoadJobsAsync();

        if (updated != null)
        {
            SelectedJob = Jobs.FirstOrDefault(x => x.Id == updated.Id);
        }
    }

    private void OpenInModule()
    {
        if (SelectedJobDetail == null || !CanOpenInModule)
        {
            return;
        }

        var pageKey = SelectedJobDetail.MachineType switch
        {
            "CNC" => "CncWorkspace",
            "LASER" => "LaserWorkspace",
            "PRINTER_3D" => "Print3dWorkspace",
            _ => null
        };

        if (pageKey == null)
        {
            ErrorMessage = "This job does not have a matching workspace in Control Center yet.";
            return;
        }

        _activeProductionJobService.SetCurrentJob(new ActiveProductionJobContext
        {
            JobId = SelectedJobDetail.Id,
            JobReference = SelectedJobDetail.JobReference,
            Title = SelectedJobDetail.Title,
            SourceType = SelectedJobDetail.SourceType,
            SourceReference = SelectedJobDetail.SourceReference,
            MachineType = SelectedJobDetail.MachineType,
            Status = SelectedJobDetail.Status,
            Description = SelectedJobDetail.Description,
            CustomerName = SelectedJobDetail.CustomerName,
            PayloadJson = SelectedJobDetail.PayloadJson,
            Attachments = SelectedJobDetail.Attachments.ToList()
        });

        _navigationService.NavigateTo(pageKey);
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

    private static string MapSourceLabel(string sourceType) => ControlCenterJobDisplay.FormatSourceType(sourceType);

    private static string MapMachineLabel(string? machineType) => ControlCenterJobDisplay.FormatMachineType(machineType);

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
