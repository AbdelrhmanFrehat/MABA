using System.Collections.ObjectModel;
using System.Windows;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class CncControllerService : ICncControllerService
{
    private readonly ILoggingService _loggingService;
    private readonly ICncProfileService _profileService;
    private readonly ICncDriverFactory _driverFactory;
    private readonly ICncCoordinateTransformService _coordinateTransformService;
    private ICncDriver _driver;
    private CncMachineState _machineState = CncMachineState.Disconnected;
    private readonly CncCoordinateSystemState _coordinateState = new();
    private string? _lastFaultReason;
    private string? _lastWarning;

    public CncControllerService(
        ILoggingService loggingService,
        ICncProfileService profileService,
        ICncDriverFactory driverFactory,
        ICncCoordinateTransformService coordinateTransformService)
    {
        _loggingService = loggingService;
        _profileService = profileService;
        _driverFactory = driverFactory;
        _coordinateTransformService = coordinateTransformService;
        _driver = CreateDriver(_profileService.ActiveProfile);

        _profileService.ActiveProfileChanged += (_, _) => HandleActiveProfileChanged();
        _profileService.ProfilesChanged += (_, _) => HandleProfilesChanged();
    }

    public ObservableCollection<LogEntry> SerialLogs => _driver.SerialLogs;
    public bool IsConnected => _driver.IsConnected;
    public string? ConnectedPort => _driver.ConnectedPort;
    public bool MotorsEnabled => _driver.MotorsEnabled;
    public bool IsHomed => _coordinateState.ReferenceState.IsHomed;
    public bool HasValidMachineReference => _coordinateState.ReferenceState.ReferenceValid;
    public CncMachineReferenceState ReferenceState => _coordinateState.ReferenceState;
    public CncCoordinateSystemState CoordinateState => _coordinateState.Clone();
    public CncMachineState MachineState => _machineState;
    public CncDeviceStatusSnapshot DeviceStatus => _driver.DeviceStatus;
    public CncDriverCapabilities DriverCapabilities => _driver.Capabilities;
    public CncMachineBounds Bounds => Config.Bounds;
    public decimal MachineX => _coordinateState.MachineX;
    public decimal MachineY => _coordinateState.MachineY;
    public decimal MachineZ => _coordinateState.MachineZ;
    public decimal WorkX => _coordinateState.WorkX;
    public decimal WorkY => _coordinateState.WorkY;
    public decimal WorkZ => _coordinateState.WorkZ;
    public decimal WorkOffsetX => _coordinateState.ActiveWorkOffset.X;
    public decimal WorkOffsetY => _coordinateState.ActiveWorkOffset.Y;
    public decimal WorkOffsetZ => _coordinateState.ActiveWorkOffset.Z;
    public string? LastFaultReason => _lastFaultReason;
    public string? LastWarning => _lastWarning;
    public CncMachineConfig Config => _profileService.ActiveProfile.ToMachineConfig();
    public event EventHandler? StateChanged;
    public event EventHandler? ConnectionLost;

    public IEnumerable<string> GetAvailablePorts() => _driver.GetAvailablePorts();

    public void Connect(string portName)
    {
        _driver.Connect(portName);
        _lastFaultReason = null;
        _lastWarning = null;
        SyncMachinePositionFromDevice();
        InitializeReferenceAfterConnect();
        UpdateState(MapDeviceStateToMachineState(DeviceStatus.DeviceState));
        SyncCoordinateState();
        NotifyStateChanged();
        AddControllerLog($"Profile '{_profileService.ActiveProfile.ProfileName}' connected on {portName}.", "Info");
    }

    public void Disconnect()
    {
        _driver.Disconnect();
        _lastFaultReason = null;
        _lastWarning = null;
        MarkReferenceUnknown(CncReferenceLostReason.Disconnect);
        UpdateState(CncMachineState.Disconnected);
        SyncCoordinateState();
        NotifyStateChanged();
        AddControllerLog("Machine disconnected.", "Info");
    }

    public string EnableMotors()
    {
        var response = _driver.EnableMotors();
        if (!IsErrorLikeResponse(response))
        {
            if (!HasValidMachineReference)
                MarkReferenceUnknown(CncReferenceLostReason.UnlockWithoutReference);
            UpdateState(CncMachineState.Idle);
        }
        SyncCoordinateState();
        NotifyStateChanged();
        return response;
    }

    public string DisableMotors()
    {
        var response = _driver.DisableMotors();
        SyncCoordinateState();
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
                _coordinateState.MachineX = Bounds.XMin;
            if (Config.HomeYEnabled)
                _coordinateState.MachineY = Bounds.YMin;
            if (Config.HomeZEnabled)
                _coordinateState.MachineZ = Bounds.ZMin;

            SyncMachinePositionFromDevice();
            MarkReferenceHomed();
            AddControllerLog("Machine reference established by homing.", "Info");
            UpdateState(CncMachineState.Idle);
        }

        SyncCoordinateState();
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

        var centerX = (Bounds.XMin + Bounds.XMax) / 2m;
        var centerY = (Bounds.YMin + Bounds.YMax) / 2m;
        var currentZ = _coordinateState.MachineZ;
        var boundsMessage = ValidateMachinePosition(centerX, centerY, currentZ);
        if (boundsMessage != null)
            throw new InvalidOperationException(boundsMessage);

        const string movingMessage = "Moving to machine center.";
        AddControllerLog(movingMessage, "Info");
        feedbackMessages.Add(movingMessage);

        var deltaX = centerX - _coordinateState.MachineX;
        var deltaY = centerY - _coordinateState.MachineY;
        var distance = (decimal)Math.Sqrt((double)((deltaX * deltaX) + (deltaY * deltaY)));
        var planned = new CncPlannedCommand
        {
            CommandText = string.Create(
                System.Globalization.CultureInfo.InvariantCulture,
                $"G0 X{centerX:0.###} Y{centerY:0.###} F{Config.MaxRapidXyMmPerMinute:0.###}"),
            SourceLineNumber = 0,
            MotionType = GcodeMotionType.Rapid,
            ExpectedEndX = centerX,
            ExpectedEndY = centerY,
            ExpectedEndZ = currentZ,
            EstimatedDistanceMm = distance,
            RequiresAck = true,
            SafetyCategory = CncCommandSafetyCategory.Motion,
            CoordinateSpace = GcodeCoordinateSpace.Machine,
            FeedRateMmPerMinute = Config.MaxRapidXyMmPerMinute,
            MotionClass = CncMotionExecutionClass.RapidTravel,
            OriginalRawLine = "Go To Center"
        };

        var ack = ExecutePlannedCommand(planned);
        if (!ack.Success)
            throw new InvalidOperationException(ack.ErrorMessage ?? ack.ResponseText ?? "Center move failed.");

        AddControllerLog($"Machine center reached at X {centerX:0.###} mm, Y {centerY:0.###} mm.", "Info");
        feedbackMessages.Add($"Machine center reached at X {centerX:0.###} mm, Y {centerY:0.###} mm.");
        NotifyStateChanged();
        return string.Join(" ", feedbackMessages);
    }

    public string SetWorkZero()
    {
        return SetWorkZeroXY();
    }

    public string SetWorkZeroX()
    {
        return SetWorkZeroInternal(updateX: true, updateY: false, updateZ: false, "Work zero X updated to current machine position.");
    }

    public string SetWorkZeroY()
    {
        return SetWorkZeroInternal(updateX: false, updateY: true, updateZ: false, "Work zero Y updated to current machine position.");
    }

    public string SetWorkZeroZ()
    {
        EnsureConnected();
        EnsureAxisSupported("Z");
        if (!HasValidMachineReference && _driver.DriverType != CncDriverType.Simulated)
            throw new InvalidOperationException("XY reference is not valid. Home the machine before setting manual Z zero.");

        _coordinateState.ActiveWorkOffset.Z = _coordinateState.MachineZ;
        _coordinateState.ReferenceState.LastZeroedAt = DateTime.UtcNow;
        _coordinateState.ReferenceState.LastZZeroedAt = DateTime.UtcNow;
        _coordinateState.ReferenceState.ZReferenceValid = true;
        _coordinateState.ReferenceState.ZReferenceState = ZReferenceState.ManualZeroSet;
        _coordinateState.ReferenceState.ZReferenceSource = ZReferenceSource.Manual;
        SyncCoordinateState();
        AddControllerLog("Manual Z zero set at the current machine position.", "Info");
        NotifyStateChanged();
        return "OK";
    }

    public string SetWorkZeroXY()
    {
        return SetWorkZeroInternal(updateX: true, updateY: true, updateZ: false, "Work zero XY updated to current machine position.");
    }

    public string SetWorkZeroXYZ()
    {
        return SetWorkZeroInternal(updateX: true, updateY: true, updateZ: true, "Work zero XYZ updated to current machine position.");
    }

    public string ClearWorkOffset()
    {
        EnsureConnected();
        _coordinateState.ActiveWorkOffset = new CncWorkOffset();
        _coordinateState.ReferenceState.LastZeroedAt = DateTime.UtcNow;
        ClearManualZReference();
        SyncCoordinateState();
        AddControllerLog("Work zero cleared back to machine zero.", "Info");
        NotifyStateChanged();
        return "OK";
    }

    public string ClearZZero()
    {
        EnsureConnected();
        _coordinateState.ActiveWorkOffset.Z = 0m;
        _coordinateState.ReferenceState.LastZeroedAt = DateTime.UtcNow;
        ClearManualZReference();
        SyncCoordinateState();
        AddControllerLog("Manual Z zero cleared.", "Info");
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
            MarkReferenceUnknown(CncReferenceLostReason.Reset);
            UpdateState(CncMachineState.Idle);
        }

        SyncCoordinateState();
        NotifyStateChanged();
        return response;
    }

    public string Stop()
    {
        var response = _driver.Stop();
        if (!IsErrorLikeResponse(response))
            UpdateState(CncMachineState.Stopped);
        else
            MarkReferenceLost(CncReferenceLostReason.Alarm);
        SyncCoordinateState();
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
        var targetX = _coordinateState.MachineX;
        var targetY = _coordinateState.MachineY;
        var targetZ = _coordinateState.MachineZ;

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
                _coordinateState.MachineX = DeviceStatus.ReportedX ?? targetX;
                _coordinateState.MachineY = DeviceStatus.ReportedY ?? targetY;
                _coordinateState.MachineZ = DeviceStatus.ReportedZ ?? targetZ;
            }
            else
            {
                _coordinateState.MachineX = targetX;
                _coordinateState.MachineY = targetY;
                _coordinateState.MachineZ = targetZ;
            }

            if (_machineState != CncMachineState.Stopped && _machineState != CncMachineState.Warning)
                UpdateState(CncMachineState.Idle);
        }

        SyncCoordinateState();
        NotifyStateChanged();
        return response;
    }

    public string MoveLinear(decimal deltaXmm, decimal deltaYmm, decimal deltaZmm)
    {
        EnsureConnected();
        EnsureMotorsEnabled();
        EnsureNoAlarmBlocking();

        var targetX = _coordinateState.MachineX + deltaXmm;
        var targetY = _coordinateState.MachineY + deltaYmm;
        var targetZ = _coordinateState.MachineZ + deltaZmm;

        var limitError = ValidateMachinePosition(targetX, targetY, targetZ);
        if (limitError != null)
        {
            _lastWarning = limitError;
            UpdateState(CncMachineState.Warning);
            AddControllerLog(limitError, "Warning");
            throw new InvalidOperationException(limitError);
        }

        if (deltaXmm != 0m)
            EnsureAxisSupported("X");
        if (deltaYmm != 0m)
            EnsureAxisSupported("Y");
        if (deltaZmm != 0m)
            EnsureAxisSupported("Z");

        UpdateState(CncMachineState.Running);
        var response = _driver.MoveLinear(deltaXmm, deltaYmm, deltaZmm);

        if (!IsErrorLikeResponse(response))
        {
            if (DeviceStatus.HasReportedPosition)
            {
                _coordinateState.MachineX = DeviceStatus.ReportedX ?? targetX;
                _coordinateState.MachineY = DeviceStatus.ReportedY ?? targetY;
                _coordinateState.MachineZ = DeviceStatus.ReportedZ ?? targetZ;
            }
            else
            {
                _coordinateState.MachineX = targetX;
                _coordinateState.MachineY = targetY;
                _coordinateState.MachineZ = targetZ;
            }

            if (_machineState != CncMachineState.Stopped && _machineState != CncMachineState.Warning)
                UpdateState(CncMachineState.Idle);
        }

        SyncCoordinateState();
        NotifyStateChanged();
        return response;
    }

    public CncControllerAckResult ExecutePlannedCommand(CncPlannedCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        EnsureConnected();
        if (command.SafetyCategory == CncCommandSafetyCategory.Motion)
            EnsureMotorsEnabled();
        EnsureNoAlarmBlocking();

        var startedAt = DateTime.UtcNow;
        UpdateState(command.SafetyCategory == CncCommandSafetyCategory.Motion
            ? CncMachineState.Running
            : _machineState);

        try
        {
            var ack = _driver.ExecutePlannedCommand(command);
            ack.SourceLineNumber ??= command.SourceLineNumber;
            ack.RoundTripMilliseconds = ack.RoundTripMilliseconds <= 0d
                ? (DateTime.UtcNow - startedAt).TotalMilliseconds
                : ack.RoundTripMilliseconds;

            if (ack.Success)
            {
                ApplySnapshotForPlannedCommand(command);
                if (_machineState != CncMachineState.Stopped && _machineState != CncMachineState.Warning)
                    UpdateState(CncMachineState.Idle);
            }
            else if (ack.IsAlarm)
            {
                _lastFaultReason = ack.ErrorMessage ?? ack.ResponseText;
                UpdateState(CncMachineState.Alarm);
            }
            else
            {
                _lastFaultReason = ack.ErrorMessage ?? ack.ResponseText;
                UpdateState(CncMachineState.Error);
            }

            NotifyStateChanged();
            return ack;
        }
        catch (Exception ex)
        {
            _lastFaultReason = ex.Message;
            UpdateState(CncMachineState.Error);
            NotifyStateChanged();

            return new CncControllerAckResult
            {
                Success = false,
                ResponseText = ex.Message,
                ErrorMessage = ex.Message,
                RoundTripMilliseconds = (DateTime.UtcNow - startedAt).TotalMilliseconds,
                SourceLineNumber = command.SourceLineNumber
            };
        }
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
        profile.RequireManualZZeroForCutting = config.RequireManualZZeroForCutting;
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

        var deltaX = targetX - _coordinateState.MachineX;
        var deltaY = targetY - _coordinateState.MachineY;
        var deltaZ = targetZ - _coordinateState.MachineZ;
        UpdateState(CncMachineState.Running);

        if (deltaX != 0m)
            ExecuteAxisMove("X", deltaX, targetX, _coordinateState.MachineY, _coordinateState.MachineZ);

        if (deltaY != 0m)
            ExecuteAxisMove("Y", deltaY, _coordinateState.MachineX, targetY, _coordinateState.MachineZ);

        if (deltaZ != 0m)
            ExecuteAxisMove("Z", deltaZ, _coordinateState.MachineX, _coordinateState.MachineY, targetZ);

        if (_machineState != CncMachineState.Stopped && _machineState != CncMachineState.Warning)
            UpdateState(CncMachineState.Idle);

        SyncCoordinateState();
    }

    private void ExecuteAxisMove(string axis, decimal deltaMm, decimal targetX, decimal targetY, decimal targetZ)
    {
        var response = _driver.Jog(axis, deltaMm);
        if (IsErrorLikeResponse(response))
            throw new InvalidOperationException(response);

        if (DeviceStatus.HasReportedPosition)
        {
            _coordinateState.MachineX = DeviceStatus.ReportedX ?? targetX;
            _coordinateState.MachineY = DeviceStatus.ReportedY ?? targetY;
            _coordinateState.MachineZ = DeviceStatus.ReportedZ ?? targetZ;
        }
        else
        {
            _coordinateState.MachineX = targetX;
            _coordinateState.MachineY = targetY;
            _coordinateState.MachineZ = targetZ;
        }

        SyncCoordinateState();
    }

    public string? ValidateMachinePosition(decimal machineX, decimal machineY, decimal machineZ)
    {
        return _coordinateTransformService.ValidateBounds(machineX, machineY, machineZ, Bounds, Config);
    }

    public string? ValidateWorkPosition(decimal workX, decimal workY, decimal workZ)
    {
        var machine = _coordinateTransformService.WorkToMachine(workX, workY, workZ, _coordinateState);
        return ValidateMachinePosition(machine.FinalMachineX, machine.FinalMachineY, machine.FinalMachineZ);
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
        {
            _lastFaultReason = reason;
            MarkReferenceLost(CncReferenceLostReason.Alarm);
        }
        else if (state is CncMachineState.Idle or CncMachineState.Completed or CncMachineState.Stopped)
        {
            _lastWarning = null;
            _lastFaultReason = null;
        }

        UpdateState(state);
        SyncCoordinateState();
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

        ResetCoordinateState();
        MarkReferenceUnknown(CncReferenceLostReason.Disconnect);
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
            ResetCoordinateState();
            MarkReferenceUnknown(CncReferenceLostReason.Disconnect);
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
        MarkReferenceLost(CncReferenceLostReason.Disconnect);
        _lastFaultReason = _driver.DeviceStatus.LastProtocolError ?? "Execution interrupted by serial disconnect.";
        UpdateState(CncMachineState.Alarm);
        SyncCoordinateState();
        RunOnUiThread(() => ConnectionLost?.Invoke(this, EventArgs.Empty));
        NotifyStateChanged();
    }

    private void ApplyDeviceSnapshot()
    {
        SyncMachinePositionFromDevice();

        if (!string.IsNullOrWhiteSpace(DeviceStatus.LastProtocolError))
        {
            _lastFaultReason = DeviceStatus.LastProtocolError;
            MarkReferenceLost(CncReferenceLostReason.Alarm);
        }

        UpdateState(MapDeviceStateToMachineState(DeviceStatus.DeviceState));
        SyncCoordinateState();
        NotifyStateChanged();
    }

    private void ApplySnapshotForPlannedCommand(CncPlannedCommand command)
    {
        if (DeviceStatus.HasReportedPosition)
        {
            SyncMachinePositionFromDevice();
            return;
        }

        if (command.ExpectedEndX.HasValue)
            _coordinateState.MachineX = command.ExpectedEndX.Value;
        if (command.ExpectedEndY.HasValue)
            _coordinateState.MachineY = command.ExpectedEndY.Value;
        if (command.ExpectedEndZ.HasValue)
            _coordinateState.MachineZ = command.ExpectedEndZ.Value;

        SyncCoordinateState();
    }

    private string SetWorkZeroInternal(bool updateX, bool updateY, bool updateZ, string successMessage)
    {
        EnsureConnected();
        if (!HasValidMachineReference)
            throw new InvalidOperationException("Machine reference is not valid. Home the machine before setting work zero.");

        if (updateX)
            _coordinateState.ActiveWorkOffset.X = _coordinateState.MachineX;
        if (updateY)
            _coordinateState.ActiveWorkOffset.Y = _coordinateState.MachineY;
        if (updateZ)
        {
            _coordinateState.ActiveWorkOffset.Z = _coordinateState.MachineZ;
            _coordinateState.ReferenceState.ZReferenceValid = true;
            _coordinateState.ReferenceState.ZReferenceState = ZReferenceState.ManualZeroSet;
            _coordinateState.ReferenceState.ZReferenceSource = ZReferenceSource.Manual;
            _coordinateState.ReferenceState.LastZZeroedAt = DateTime.UtcNow;
        }

        _coordinateState.ReferenceState.LastZeroedAt = DateTime.UtcNow;
        SyncCoordinateState();
        AddControllerLog(successMessage, "Info");
        NotifyStateChanged();
        return "OK";
    }

    private void InitializeReferenceAfterConnect()
    {
        if (_driver.DriverType == CncDriverType.Simulated)
        {
            _coordinateState.ReferenceState.IsHomed = true;
            _coordinateState.ReferenceState.ReferenceValid = true;
            _coordinateState.ReferenceState.ReferenceLostReason = CncReferenceLostReason.None;
            _coordinateState.ReferenceState.HomedAxes = GetSupportedReferenceAxes();
            _coordinateState.ReferenceState.LastHomedAt ??= DateTime.UtcNow;
            _coordinateState.ReferenceState.ZReferenceValid = Config.SupportsZAxis;
            _coordinateState.ReferenceState.ZReferenceState = Config.SupportsZAxis ? ZReferenceState.ManualZeroSet : ZReferenceState.NotSupported;
            _coordinateState.ReferenceState.ZReferenceSource = Config.SupportsZAxis ? ZReferenceSource.Manual : ZReferenceSource.None;
            _coordinateState.ReferenceState.LastZZeroedAt ??= Config.SupportsZAxis ? DateTime.UtcNow : null;
            return;
        }

        MarkReferenceUnknown(CncReferenceLostReason.UnknownOnConnect);
    }

    private void MarkReferenceHomed()
    {
        _coordinateState.ReferenceState.IsHomed = true;
        _coordinateState.ReferenceState.ReferenceValid = true;
        _coordinateState.ReferenceState.ReferenceLostReason = CncReferenceLostReason.None;
        _coordinateState.ReferenceState.HomedAxes = GetSupportedReferenceAxes();
        _coordinateState.ReferenceState.LastHomedAt = DateTime.UtcNow;
    }

    private void MarkReferenceUnknown(CncReferenceLostReason reason)
    {
        _coordinateState.ReferenceState.IsHomed = false;
        _coordinateState.ReferenceState.ReferenceValid = false;
        _coordinateState.ReferenceState.HomedAxes = CncHomedAxes.None;
        _coordinateState.ReferenceState.ReferenceLostReason = reason;
        MarkZReferenceLost(reason);
    }

    private void MarkReferenceLost(CncReferenceLostReason reason)
    {
        _coordinateState.ReferenceState.ReferenceValid = false;
        _coordinateState.ReferenceState.IsHomed = false;
        _coordinateState.ReferenceState.ReferenceLostReason = reason;
        _coordinateState.ReferenceState.HomedAxes = CncHomedAxes.None;
        MarkZReferenceLost(reason);
    }

    private void ResetCoordinateState()
    {
        _coordinateState.MachineX = Bounds.XMin;
        _coordinateState.MachineY = Bounds.YMin;
        _coordinateState.MachineZ = Bounds.ZMin;
        _coordinateState.ActiveWorkOffset = new CncWorkOffset();
        _coordinateState.JobPlacementOffset = new CncJobPlacementOffset();
        SyncCoordinateState();
    }

    private void SyncMachinePositionFromDevice()
    {
        if (!DeviceStatus.HasReportedPosition)
            return;

        _coordinateState.MachineX = DeviceStatus.ReportedX ?? _coordinateState.MachineX;
        _coordinateState.MachineY = DeviceStatus.ReportedY ?? _coordinateState.MachineY;
        _coordinateState.MachineZ = DeviceStatus.ReportedZ ?? _coordinateState.MachineZ;
        SyncCoordinateState();
    }

    private void SyncCoordinateState()
    {
        var refreshed = _coordinateTransformService.CreateState(
            _coordinateState.MachineX,
            _coordinateState.MachineY,
            _coordinateState.MachineZ,
            _coordinateState.ActiveWorkOffset,
            _coordinateState.JobPlacementOffset,
            _coordinateState.ReferenceState,
            _coordinateState.CoordinateMode);

        _coordinateState.WorkX = refreshed.WorkX;
        _coordinateState.WorkY = refreshed.WorkY;
        _coordinateState.WorkZ = refreshed.WorkZ;
    }

    private CncHomedAxes GetSupportedReferenceAxes()
    {
        var axes = CncHomedAxes.None;
        if (Config.HomeXEnabled)
            axes |= CncHomedAxes.X;
        if (Config.HomeYEnabled)
            axes |= CncHomedAxes.Y;
        return axes;
    }

    private void ClearManualZReference()
    {
        _coordinateState.ReferenceState.ZReferenceValid = false;
        _coordinateState.ReferenceState.ZReferenceState = Config.SupportsZAxis ? ZReferenceState.Unknown : ZReferenceState.NotSupported;
        _coordinateState.ReferenceState.ZReferenceSource = ZReferenceSource.None;
    }

    private void MarkZReferenceLost(CncReferenceLostReason reason)
    {
        if (!Config.SupportsZAxis)
        {
            _coordinateState.ReferenceState.ZReferenceValid = false;
            _coordinateState.ReferenceState.ZReferenceState = ZReferenceState.NotSupported;
            _coordinateState.ReferenceState.ZReferenceSource = ZReferenceSource.None;
            return;
        }

        _coordinateState.ReferenceState.ZReferenceValid = false;
        _coordinateState.ReferenceState.ZReferenceSource = _coordinateState.ReferenceState.ZReferenceSource == ZReferenceSource.None
            ? ZReferenceSource.Manual
            : _coordinateState.ReferenceState.ZReferenceSource;
        _coordinateState.ReferenceState.ZReferenceState = reason switch
        {
            CncReferenceLostReason.Disconnect => ZReferenceState.LostAfterDisconnect,
            CncReferenceLostReason.Alarm => ZReferenceState.LostAfterAlarm,
            CncReferenceLostReason.FaultRecoveryRequired => ZReferenceState.LostAfterAlarm,
            _ => ZReferenceState.Unknown
        };
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
               || response.StartsWith("ALARM:", StringComparison.OrdinalIgnoreCase)
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
            RequireManualZZeroForCutting = profile.RequireManualZZeroForCutting,
            SoftLimitsEnabled = profile.SoftLimitsEnabled,
            JogPresets = profile.JogPresets.ToList(),
            SafeTravelZMm = profile.SafeTravelZMm,
            MaxFeedXyMmPerMinute = profile.MaxFeedXyMmPerMinute,
            MaxRapidXyMmPerMinute = profile.MaxRapidXyMmPerMinute,
            MaxFeedZMmPerMinute = profile.MaxFeedZMmPerMinute,
            MaxPlungeZMmPerMinute = profile.MaxPlungeZMmPerMinute,
            ParkXMm = profile.ParkXMm,
            ParkYMm = profile.ParkYMm,
            ParkZMm = profile.ParkZMm,
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
