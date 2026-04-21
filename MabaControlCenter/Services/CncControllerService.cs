using System.Collections.ObjectModel;
using System.Windows;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class CncControllerService : ICncControllerService
{
    private readonly ILoggingService _loggingService;
    private readonly ICncProfileService _profileService;
    private readonly ICncDriverFactory _driverFactory;
    private ICncDriver _driver;
    private CncMachineState _machineState = CncMachineState.Disconnected;
    private decimal _machineX;
    private decimal _machineY;
    private decimal _machineZ;
    private decimal _workOffsetX;
    private decimal _workOffsetY;
    private decimal _workOffsetZ;
    private bool _isHomed;
    private string? _lastFaultReason;
    private string? _lastWarning;

    public CncControllerService(
        ILoggingService loggingService,
        ICncProfileService profileService,
        ICncDriverFactory driverFactory)
    {
        _loggingService = loggingService;
        _profileService = profileService;
        _driverFactory = driverFactory;
        _driver = CreateDriver(_profileService.ActiveProfile);

        _profileService.ActiveProfileChanged += (_, _) => HandleActiveProfileChanged();
        _profileService.ProfilesChanged += (_, _) => HandleProfilesChanged();
    }

    public ObservableCollection<LogEntry> SerialLogs => _driver.SerialLogs;
    public bool IsConnected => _driver.IsConnected;
    public string? ConnectedPort => _driver.ConnectedPort;
    public bool MotorsEnabled => _driver.MotorsEnabled;
    public bool IsHomed => _isHomed;
    public bool HasValidMachineReference => _isHomed;
    public CncMachineState MachineState => _machineState;
    public CncDeviceStatusSnapshot DeviceStatus => _driver.DeviceStatus;
    public CncDriverCapabilities DriverCapabilities => _driver.Capabilities;
    public CncMachineBounds Bounds => Config.Bounds;
    public decimal MachineX => _machineX;
    public decimal MachineY => _machineY;
    public decimal MachineZ => _machineZ;
    public decimal WorkX => _machineX - _workOffsetX;
    public decimal WorkY => _machineY - _workOffsetY;
    public decimal WorkZ => _machineZ - _workOffsetZ;
    public decimal WorkOffsetX => _workOffsetX;
    public decimal WorkOffsetY => _workOffsetY;
    public decimal WorkOffsetZ => _workOffsetZ;
    public string? LastFaultReason => _lastFaultReason;
    public string? LastWarning => _lastWarning;
    public CncMachineConfig Config => _profileService.ActiveProfile.ToMachineConfig();
    public event EventHandler? StateChanged;
    public event EventHandler? ConnectionLost;

    public IEnumerable<string> GetAvailablePorts() => _driver.GetAvailablePorts();

    public void Connect(string portName)
    {
        _driver.Connect(portName);
        _isHomed = false;
        _lastFaultReason = null;
        _lastWarning = null;
        UpdateState(MapDeviceStateToMachineState(DeviceStatus.DeviceState));
        AddControllerLog($"Profile '{_profileService.ActiveProfile.ProfileName}' connected on {portName}.", "Info");
    }

    public void Disconnect()
    {
        _driver.Disconnect();
        _isHomed = false;
        _lastFaultReason = null;
        _lastWarning = null;
        UpdateState(CncMachineState.Disconnected);
        AddControllerLog("Machine disconnected.", "Info");
    }

    public string EnableMotors()
    {
        var response = _driver.EnableMotors();
        if (!IsErrorLikeResponse(response))
            UpdateState(CncMachineState.Idle);
        NotifyStateChanged();
        return response;
    }

    public string DisableMotors()
    {
        var response = _driver.DisableMotors();
        NotifyStateChanged();
        return response;
    }

    public string AutoHome()
    {
        EnsureConnected();
        EnsureNoAlarmBlocking();

        if (!Config.HomeXEnabled && !Config.HomeYEnabled)
            throw new InvalidOperationException("The active profile does not allow homing on X or Y.");

        UpdateState(CncMachineState.Homing);
        var response = _driver.AutoHome();

        if (!IsErrorLikeResponse(response))
        {
            if (Config.HomeXEnabled)
                _machineX = Bounds.XMin;
            if (Config.HomeYEnabled)
                _machineY = Bounds.YMin;

            if (DeviceStatus.HasReportedPosition)
            {
                _machineX = DeviceStatus.ReportedX ?? _machineX;
                _machineY = DeviceStatus.ReportedY ?? _machineY;
                _machineZ = DeviceStatus.ReportedZ ?? _machineZ;
            }

            _isHomed = true;
            AddControllerLog("Machine reference established by homing.", "Info");
            UpdateState(CncMachineState.Idle);
        }

        NotifyStateChanged();
        return response;
    }

    public string GoToCenter()
    {
        EnsureConnected();
        EnsureMotorsEnabled();
        EnsureNoAlarmBlocking();
        EnsureAxisSupported("X");
        EnsureAxisSupported("Y");

        if (Bounds.XMax <= Bounds.XMin || Bounds.YMax <= Bounds.YMin)
            throw new InvalidOperationException("Machine profile bounds are invalid. Check X/Y machine limits before moving to center.");

        var feedbackMessages = new List<string>();
        if (!HasValidMachineReference)
        {
            if (!Config.HomeXEnabled && !Config.HomeYEnabled)
                throw new InvalidOperationException("Machine reference is unknown and the active profile does not allow X/Y homing.");

            const string homingMessage = "Machine reference unknown. Homing first before moving to center.";
            AddControllerLog(homingMessage, "Warning");
            feedbackMessages.Add(homingMessage);

            var homeResponse = AutoHome();
            if (IsErrorLikeResponse(homeResponse))
                throw new InvalidOperationException($"Center move aborted because homing failed: {homeResponse}");
        }

        var centerX = Bounds.XMax / 2m;
        var centerY = Bounds.YMax / 2m;
        var currentZ = _machineZ;
        var boundsMessage = ValidateMachinePosition(centerX, centerY, currentZ);
        if (boundsMessage != null)
            throw new InvalidOperationException(boundsMessage);

        const string movingMessage = "Moving to machine center.";
        AddControllerLog(movingMessage, "Info");
        feedbackMessages.Add(movingMessage);

        MoveToMachineCoordinates(centerX, centerY, currentZ);
        AddControllerLog($"Machine center reached at X {centerX:0.###} mm, Y {centerY:0.###} mm.", "Info");
        feedbackMessages.Add($"Machine center reached at X {centerX:0.###} mm, Y {centerY:0.###} mm.");
        NotifyStateChanged();
        return string.Join(" ", feedbackMessages);
    }

    public string SetWorkZero()
    {
        EnsureConnected();
        _workOffsetX = _machineX;
        _workOffsetY = _machineY;
        _workOffsetZ = _machineZ;
        AddControllerLog("Work zero updated to current machine position.", "Info");
        NotifyStateChanged();
        return "OK";
    }

    public string ResetState()
    {
        return ResetAlarm();
    }

    public string ResetAlarm()
    {
        if (!IsConnected)
        {
            UpdateState(CncMachineState.Disconnected);
            _lastFaultReason = "Reconnect the machine before resetting alarms.";
            NotifyStateChanged();
            return $"ERR:{_lastFaultReason}";
        }

        var response = _driver.ResetAlarm();
        if (!IsErrorLikeResponse(response))
        {
            _lastFaultReason = null;
            _lastWarning = null;
            UpdateState(CncMachineState.Idle);
        }

        NotifyStateChanged();
        return response;
    }

    public string Stop()
    {
        var response = _driver.Stop();
        if (!IsErrorLikeResponse(response))
            UpdateState(CncMachineState.Stopped);
        NotifyStateChanged();
        return response;
    }

    public string RefreshStatus()
    {
        var response = _driver.RefreshStatus();
        ApplyDeviceSnapshot();
        return response;
    }

    public string Jog(string axis, decimal deltaMm)
    {
        EnsureConnected();
        EnsureMotorsEnabled();
        EnsureNoAlarmBlocking();
        EnsureAxisSupported(axis);

        var normalizedAxis = (axis ?? string.Empty).Trim().ToUpperInvariant();
        var targetX = _machineX;
        var targetY = _machineY;
        var targetZ = _machineZ;

        switch (normalizedAxis)
        {
            case "X":
                targetX += deltaMm;
                break;
            case "Y":
                targetY += deltaMm;
                break;
            case "Z":
                targetZ += deltaMm;
                break;
        }

        var limitError = ValidateMachinePosition(targetX, targetY, targetZ);
        if (limitError != null)
        {
            _lastWarning = limitError;
            UpdateState(CncMachineState.Warning);
            AddControllerLog(limitError, "Warning");
            throw new InvalidOperationException(limitError);
        }

        UpdateState(CncMachineState.Running);
        var response = _driver.Jog(normalizedAxis, deltaMm);

        if (!IsErrorLikeResponse(response))
        {
            if (DeviceStatus.HasReportedPosition)
            {
                _machineX = DeviceStatus.ReportedX ?? targetX;
                _machineY = DeviceStatus.ReportedY ?? targetY;
                _machineZ = DeviceStatus.ReportedZ ?? targetZ;
            }
            else
            {
                _machineX = targetX;
                _machineY = targetY;
                _machineZ = targetZ;
            }

            if (_machineState != CncMachineState.Stopped && _machineState != CncMachineState.Warning)
                UpdateState(CncMachineState.Idle);
        }

        NotifyStateChanged();
        return response;
    }

    public void UpdateConfig(CncMachineConfig config)
    {
        var profile = CloneProfile(_profileService.ActiveProfile);
        profile.BaudRate = config.BaudRate;
        profile.XStepsPerMm = config.XStepsPerMm;
        profile.YStepsPerMm = config.YStepsPerMm;
        profile.ZStepsPerMm = config.ZStepsPerMm;
        profile.XMinMm = config.XMinMm;
        profile.XLimitMm = config.XLimitMm;
        profile.YMinMm = config.YMinMm;
        profile.YLimitMm = config.YLimitMm;
        profile.ZMinMm = config.ZMinMm;
        profile.ZLimitMm = config.ZLimitMm;
        profile.HomeOriginConvention = config.HomeOriginConvention;
        profile.HomeXEnabled = config.HomeXEnabled;
        profile.HomeYEnabled = config.HomeYEnabled;
        profile.HomeZEnabled = config.HomeZEnabled;
        profile.SupportsXAxis = config.SupportsXAxis;
        profile.SupportsYAxis = config.SupportsYAxis;
        profile.SupportsZAxis = config.SupportsZAxis;
        profile.SoftLimitsEnabled = config.SoftLimitsEnabled;
        profile.DriverType = config.DriverType;
        profile.JogPresets = config.JogPresets.ToList();
        profile.VisualizationWidthMm = config.VisualizationWidthMm;
        profile.VisualizationHeightMm = config.VisualizationHeightMm;
        profile.VisualizationDepthMm = config.VisualizationDepthMm;

        _profileService.SaveProfile(profile);
        _driver.ApplyProfile(_profileService.ActiveProfile);
        AddControllerLog($"Active profile '{profile.ProfileName}' updated.", "Info");
        NotifyStateChanged();
    }

    private void MoveToMachineCoordinates(decimal targetX, decimal targetY, decimal targetZ)
    {
        var limitError = ValidateMachinePosition(targetX, targetY, targetZ);
        if (limitError != null)
            throw new InvalidOperationException(limitError);

        var deltaX = targetX - _machineX;
        var deltaY = targetY - _machineY;
        var deltaZ = targetZ - _machineZ;
        UpdateState(CncMachineState.Running);

        if (deltaX != 0m)
            ExecuteAxisMove("X", deltaX, targetX, _machineY, _machineZ);

        if (deltaY != 0m)
            ExecuteAxisMove("Y", deltaY, _machineX, targetY, _machineZ);

        if (deltaZ != 0m)
            ExecuteAxisMove("Z", deltaZ, _machineX, _machineY, targetZ);

        if (_machineState != CncMachineState.Stopped && _machineState != CncMachineState.Warning)
            UpdateState(CncMachineState.Idle);
    }

    private void ExecuteAxisMove(string axis, decimal deltaMm, decimal targetX, decimal targetY, decimal targetZ)
    {
        var response = _driver.Jog(axis, deltaMm);
        if (IsErrorLikeResponse(response))
            throw new InvalidOperationException(response);

        if (DeviceStatus.HasReportedPosition)
        {
            _machineX = DeviceStatus.ReportedX ?? targetX;
            _machineY = DeviceStatus.ReportedY ?? targetY;
            _machineZ = DeviceStatus.ReportedZ ?? targetZ;
        }
        else
        {
            _machineX = targetX;
            _machineY = targetY;
            _machineZ = targetZ;
        }
    }

    public string? ValidateMachinePosition(decimal machineX, decimal machineY, decimal machineZ)
    {
        if (Config.SoftLimitsEnabled)
        {
            if (machineX < Bounds.XMin || machineX > Bounds.XMax)
                return $"Bounds violation: X {machineX:0.###} mm is outside [{Bounds.XMin:0.###}, {Bounds.XMax:0.###}] mm.";
            if (machineY < Bounds.YMin || machineY > Bounds.YMax)
                return $"Bounds violation: Y {machineY:0.###} mm is outside [{Bounds.YMin:0.###}, {Bounds.YMax:0.###}] mm.";
            if (machineZ < Bounds.ZMin || machineZ > Bounds.ZMax)
                return $"Bounds violation: Z {machineZ:0.###} mm is outside [{Bounds.ZMin:0.###}, {Bounds.ZMax:0.###}] mm.";
        }

        if (!Config.SupportsXAxis && machineX != _machineX)
            return "The active machine profile does not support X axis motion.";
        if (!Config.SupportsYAxis && machineY != _machineY)
            return "The active machine profile does not support Y axis motion.";
        if (!Config.SupportsZAxis && machineZ != _machineZ)
            return "The active machine profile does not support Z axis motion.";

        return null;
    }

    public string? ValidateWorkPosition(decimal workX, decimal workY, decimal workZ)
    {
        return ValidateMachinePosition(workX + _workOffsetX, workY + _workOffsetY, workZ + _workOffsetZ);
    }

    public void ClearWarning()
    {
        _lastWarning = null;
        if (_machineState == CncMachineState.Warning)
            UpdateState(IsConnected ? CncMachineState.Idle : CncMachineState.Disconnected);
        NotifyStateChanged();
    }

    public void SetMachineState(CncMachineState state, string? reason = null)
    {
        if (state == CncMachineState.Warning)
            _lastWarning = reason;
        else if (state is CncMachineState.Alarm or CncMachineState.Error)
            _lastFaultReason = reason;
        else if (state is CncMachineState.Idle or CncMachineState.Completed or CncMachineState.Stopped)
        {
            _lastWarning = null;
            _lastFaultReason = null;
        }

        UpdateState(state);
        if (!string.IsNullOrWhiteSpace(reason))
            AddControllerLog(reason, state is CncMachineState.Warning ? "Warning" : "Error");
    }

    private ICncDriver CreateDriver(CncMachineProfile profile)
    {
        var driver = _driverFactory.CreateDriver(profile);
        driver.StateChanged += OnDriverStateChanged;
        driver.ConnectionLost += OnDriverConnectionLost;
        return driver;
    }

    private void HandleActiveProfileChanged()
    {
        var wasConnected = IsConnected;
        if (wasConnected)
            _driver.Disconnect();

        _driver.StateChanged -= OnDriverStateChanged;
        _driver.ConnectionLost -= OnDriverConnectionLost;
        _driver = CreateDriver(_profileService.ActiveProfile);

        _isHomed = false;
        _lastWarning = null;
        _lastFaultReason = null;
        UpdateState(CncMachineState.Disconnected);
        AddControllerLog($"Active CNC profile switched to '{_profileService.ActiveProfile.ProfileName}'.", "Info");
        NotifyStateChanged();
    }

    private void HandleProfilesChanged()
    {
        if (_driver.DriverType != _profileService.ActiveProfile.DriverType)
        {
            var wasConnected = IsConnected;
            if (wasConnected)
                _driver.Disconnect();

            _driver.StateChanged -= OnDriverStateChanged;
            _driver.ConnectionLost -= OnDriverConnectionLost;
            _driver = CreateDriver(_profileService.ActiveProfile);
            _isHomed = false;
            _lastWarning = null;
            _lastFaultReason = null;
            UpdateState(CncMachineState.Disconnected);
            AddControllerLog($"Driver switched to {_profileService.ActiveProfile.DriverType}.", "Info");
        }
        else
        {
            _driver.ApplyProfile(_profileService.ActiveProfile);
        }

        NotifyStateChanged();
    }

    private void OnDriverStateChanged(object? sender, EventArgs e)
    {
        ApplyDeviceSnapshot();
    }

    private void OnDriverConnectionLost(object? sender, EventArgs e)
    {
        _isHomed = false;
        _lastFaultReason = _driver.DeviceStatus.LastProtocolError ?? "Execution interrupted by serial disconnect.";
        UpdateState(CncMachineState.Alarm);
        RunOnUiThread(() => ConnectionLost?.Invoke(this, EventArgs.Empty));
        NotifyStateChanged();
    }

    private void ApplyDeviceSnapshot()
    {
        if (DeviceStatus.HasReportedPosition)
        {
            _machineX = DeviceStatus.ReportedX ?? _machineX;
            _machineY = DeviceStatus.ReportedY ?? _machineY;
            _machineZ = DeviceStatus.ReportedZ ?? _machineZ;
        }

        if (!string.IsNullOrWhiteSpace(DeviceStatus.LastProtocolError))
            _lastFaultReason = DeviceStatus.LastProtocolError;

        UpdateState(MapDeviceStateToMachineState(DeviceStatus.DeviceState));
        NotifyStateChanged();
    }

    private void EnsureConnected()
    {
        if (!IsConnected)
            throw new InvalidOperationException("Connect the CNC machine first.");
    }

    private void EnsureMotorsEnabled()
    {
        if (!MotorsEnabled)
            throw new InvalidOperationException("Enable motors before sending motion commands.");
    }

    private void EnsureNoAlarmBlocking()
    {
        if (_machineState is CncMachineState.Alarm or CncMachineState.Error)
            throw new InvalidOperationException(_lastFaultReason ?? "Clear the alarm or error state before continuing.");
    }

    private void EnsureAxisSupported(string axis)
    {
        var normalized = (axis ?? string.Empty).Trim().ToUpperInvariant();
        var supported = normalized switch
        {
            "X" => Config.SupportsXAxis,
            "Y" => Config.SupportsYAxis,
            "Z" => Config.SupportsZAxis,
            _ => false
        };

        if (!supported)
            throw new InvalidOperationException($"The active machine profile does not support {normalized} axis motion.");
    }

    private void AddControllerLog(string message, string status)
    {
        RunOnUiThread(() => SerialLogs.Insert(0, new LogEntry
        {
            Timestamp = DateTime.Now,
            Direction = "Controller",
            Message = message,
            Status = status
        }));
        _loggingService.AddLog("CNC Controller", message, status);
    }

    private void UpdateState(CncMachineState state)
    {
        _machineState = state;
    }

    private void NotifyStateChanged()
    {
        RunOnUiThread(() => StateChanged?.Invoke(this, EventArgs.Empty));
    }

    private static bool IsErrorLikeResponse(string response)
    {
        return response.StartsWith("ERR:", StringComparison.OrdinalIgnoreCase)
               || response.Contains("LIMIT HIT", StringComparison.OrdinalIgnoreCase);
    }

    private static CncMachineState MapDeviceStateToMachineState(CncDeviceState? state)
    {
        return state switch
        {
            CncDeviceState.Ready => CncMachineState.Idle,
            CncDeviceState.Idle => CncMachineState.Idle,
            CncDeviceState.Running => CncMachineState.Running,
            CncDeviceState.Homing => CncMachineState.Homing,
            CncDeviceState.Paused => CncMachineState.Paused,
            CncDeviceState.Stopped => CncMachineState.Stopped,
            CncDeviceState.Alarm => CncMachineState.Alarm,
            CncDeviceState.Error => CncMachineState.Error,
            CncDeviceState.LimitHit => CncMachineState.Alarm,
            CncDeviceState.Disconnected => CncMachineState.Disconnected,
            _ => CncMachineState.Idle
        };
    }

    private static CncMachineProfile CloneProfile(CncMachineProfile profile)
    {
        return new CncMachineProfile
        {
            ProfileId = profile.ProfileId,
            ProfileName = profile.ProfileName,
            MachineType = profile.MachineType,
            Description = profile.Description,
            Notes = profile.Notes,
            IsDefault = profile.IsDefault,
            IsBuiltIn = profile.IsBuiltIn,
            MachineDefinitionId = profile.MachineDefinitionId,
            MachineDefinitionVersion = profile.MachineDefinitionVersion,
            DefinitionSnapshot = profile.DefinitionSnapshot,
            CompatibilityState = profile.CompatibilityState,
            DriverType = profile.DriverType,
            BaudRate = profile.BaudRate,
            XStepsPerMm = profile.XStepsPerMm,
            YStepsPerMm = profile.YStepsPerMm,
            ZStepsPerMm = profile.ZStepsPerMm,
            XMinMm = profile.XMinMm,
            XLimitMm = profile.XLimitMm,
            YMinMm = profile.YMinMm,
            YLimitMm = profile.YLimitMm,
            ZMinMm = profile.ZMinMm,
            ZLimitMm = profile.ZLimitMm,
            HomeOriginConvention = profile.HomeOriginConvention,
            HomeXEnabled = profile.HomeXEnabled,
            HomeYEnabled = profile.HomeYEnabled,
            HomeZEnabled = profile.HomeZEnabled,
            SupportsXAxis = profile.SupportsXAxis,
            SupportsYAxis = profile.SupportsYAxis,
            SupportsZAxis = profile.SupportsZAxis,
            SoftLimitsEnabled = profile.SoftLimitsEnabled,
            JogPresets = profile.JogPresets.ToList(),
            VisualizationWidthMm = profile.VisualizationWidthMm,
            VisualizationHeightMm = profile.VisualizationHeightMm,
            VisualizationDepthMm = profile.VisualizationDepthMm
        };
    }

    private static void RunOnUiThread(Action action)
    {
        if (Application.Current?.Dispatcher == null || Application.Current.Dispatcher.CheckAccess())
        {
            action();
            return;
        }

        Application.Current.Dispatcher.Invoke(action);
    }
}
