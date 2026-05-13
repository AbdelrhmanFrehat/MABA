using System.Collections.ObjectModel;
using System.Windows;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class SimulatedCncDriver : ICncDriver
{
    private readonly ILoggingService _loggingService;
    private CncMachineProfile _profile;
    private bool _isConnected;
    private bool _motorsEnabled;

    public SimulatedCncDriver(ILoggingService loggingService, CncMachineProfile profile)
    {
        _loggingService = loggingService;
        _profile = profile;
        DeviceStatus = new CncDeviceStatusSnapshot();
        Capabilities = new CncDriverCapabilities
        {
            SupportsZHoming = false,
            SupportsAcknowledgements = true,
            SupportsLivePositionReporting = true,
            SupportsCombinedXyMove = false,
            SupportsPause = true,
            SupportsAlarmReset = true,
            SupportsWorkCoordinateSystem = true,
            VisualizationModelType = "Gantry3Axis"
        };
    }

    public ObservableCollection<LogEntry> SerialLogs { get; } = new();
    public CncDriverType DriverType => CncDriverType.Simulated;
    public CncDriverCapabilities Capabilities { get; }
    public CncDeviceStatusSnapshot DeviceStatus { get; }
    public bool IsConnected => _isConnected;
    public string? ConnectedPort => _isConnected ? "SIMULATION" : null;
    public bool MotorsEnabled => _motorsEnabled;
    public event EventHandler? StateChanged;
#pragma warning disable CS0067
    public event EventHandler? ConnectionLost;
#pragma warning restore CS0067

    public IEnumerable<string> GetAvailablePorts() => new[] { "SIMULATION" };

    public void ApplyProfile(CncMachineProfile profile)
    {
        _profile = profile;
        AddDriverLog($"Simulation profile applied: {_profile.ProfileName}.", "Info");
        NotifyStateChanged();
    }

    public void Connect(string portName)
    {
        if (_isConnected)
            return;

        _isConnected = true;
        _motorsEnabled = false;
        ResetStatus();
        SimulateDelay(180);
        DeviceStatus.IsResponsive = true;
        DeviceStatus.IsReady = true;
        DeviceStatus.IsLocked = true;
        DeviceStatus.IsAlarmed = false;
        DeviceStatus.DeviceState = CncDeviceState.Ready;
        DeviceStatus.LastAcknowledgement = "READY";
        DeviceStatus.LastAcknowledgedAt = DateTime.Now;
        DeviceStatus.LastStatusText = "STATUS:IDLE";
        DeviceStatus.FirmwareVersion = "SIM-1.0";
        DeviceStatus.ProtocolVersion = "SIMULATION";
        DeviceStatus.FirmwareIdentity = new CncFirmwareIdentity
        {
            FirmwareName = "MABA Simulation Firmware",
            FirmwareVersion = "SIM-1.0",
            ProtocolName = "SIMULATION",
            ProtocolVersion = new CncProtocolVersion
            {
                ProtocolName = "SIMULATION",
                RawVersion = "SIMULATION",
                Major = 1,
                Minor = 0
            },
            Confidence = CncCapabilityConfidence.Verified,
            Capabilities = new CncFirmwareCapabilities
            {
                SupportsStatusQuery = true,
                SupportsUnlock = true,
                SupportsHoming = true,
                SupportsJog = true,
                SupportsG0G1 = true,
                SupportsG2G3 = true,
                SupportsSpindleOnOff = true,
                SupportsSpindleSpeed = false,
                SupportsFeedHold = true,
                SupportsSoftwareStop = true,
                SupportsWorkOffsets = true,
                SupportsLimitReporting = true,
                SupportsPositionReporting = true,
                SupportsFirmwareUpload = false,
                SupportedAxes = new List<string> { "X", "Y", "Z" },
                WorkspaceLimitX = _profile.XLimitMm,
                WorkspaceLimitY = _profile.YLimitMm,
                WorkspaceLimitZ = _profile.ZLimitMm
            }
        };
        DeviceStatus.FirmwareCompatibility = new CncFirmwareCompatibilityResult
        {
            Status = CncFirmwareCompatibilityStatus.Compatible
        };
        DeviceStatus.ReportedX = _profile.XMinMm;
        DeviceStatus.ReportedY = _profile.YMinMm;
        DeviceStatus.ReportedZ = _profile.ZMinMm;
        DeviceStatus.HasReportedPosition = true;
        AddDriverLog("Simulated driver connected.", "Info");
        AddProtocolLog("Simulated handshake OK.", "Info");
        NotifyStateChanged();
    }

    public void Disconnect()
    {
        if (!_isConnected)
            return;

        _isConnected = false;
        _motorsEnabled = false;
        DeviceStatus.IsResponsive = false;
        DeviceStatus.IsReady = false;
        DeviceStatus.DeviceState = CncDeviceState.Disconnected;
        DeviceStatus.LastStatusText = "Disconnected";
        AddDriverLog("Simulated driver disconnected.", "Info");
        NotifyStateChanged();
    }

    public string EnableMotors()
    {
        EnsureConnected();
        SimulateDelay(120);
        _motorsEnabled = true;
        DeviceStatus.IsLocked = false;
        DeviceStatus.DeviceState = CncDeviceState.Idle;
        return Complete("ENABLE", "OK");
    }

    public string DisableMotors()
    {
        EnsureConnected();
        SimulateDelay(120);
        _motorsEnabled = false;
        DeviceStatus.IsLocked = true;
        DeviceStatus.DeviceState = CncDeviceState.Idle;
        return Complete("DISABLE", "OK");
    }

    public string AutoHome()
    {
        EnsureConnected();
        EnsureMotorsEnabled();
        DeviceStatus.DeviceState = CncDeviceState.Homing;
        DeviceStatus.IsLocked = true;
        DeviceStatus.LastStatusText = "STATUS:HOMING";
        AddProtocolLog("Simulated home started.", "Info");
        NotifyStateChanged();
        SimulateDelay(450);

        if (_profile.HomeXEnabled)
            DeviceStatus.ReportedX = _profile.XMinMm;
        if (_profile.HomeYEnabled)
            DeviceStatus.ReportedY = _profile.YMinMm;
        if (_profile.HomeZEnabled)
            DeviceStatus.ReportedZ = _profile.ZMinMm;

        DeviceStatus.DeviceState = CncDeviceState.Idle;
        DeviceStatus.IsLocked = false;
        DeviceStatus.LastStatusText = "HOME DONE";
        DeviceStatus.LastAcknowledgement = "HOME DONE";
        DeviceStatus.LastAcknowledgedAt = DateTime.Now;
        AddProtocolLog("Simulated home completed.", "Info");
        NotifyStateChanged();
        return "HOME DONE";
    }

    public string ResetAlarm()
    {
        EnsureConnected();
        SimulateDelay(100);
        DeviceStatus.LastProtocolError = null;
        DeviceStatus.IsAlarmed = false;
        DeviceStatus.IsLocked = false;
        DeviceStatus.DeviceState = CncDeviceState.Idle;
        return Complete("RESET", "OK");
    }

    public string Stop()
    {
        EnsureConnected();
        SimulateDelay(80);
        DeviceStatus.IsAlarmed = true;
        DeviceStatus.IsLocked = true;
        DeviceStatus.DeviceState = CncDeviceState.Stopped;
        DeviceStatus.LastStatusText = "STOPPED";
        DeviceStatus.LastAcknowledgement = "STOPPED";
        DeviceStatus.LastAcknowledgedAt = DateTime.Now;
        AddProtocolLog("Simulated stop acknowledged.", "Warning");
        NotifyStateChanged();
        return "STOPPED";
    }

    public string RefreshStatus()
    {
        EnsureConnected();
        SimulateDelay(60);
        var statusText = $"STATUS:{FormatStatus(DeviceStatus.DeviceState)}";
        DeviceStatus.LastStatusText = statusText;
        DeviceStatus.LastAcknowledgement = statusText;
        DeviceStatus.LastAcknowledgedAt = DateTime.Now;
        AddProtocolLog($"Simulated status: {statusText}.", "Info");
        NotifyStateChanged();
        return statusText;
    }

    public string Jog(string axis, decimal deltaMm)
    {
        EnsureConnected();
        EnsureMotorsEnabled();

        var normalizedAxis = (axis ?? string.Empty).Trim().ToUpperInvariant();
        DeviceStatus.DeviceState = CncDeviceState.Running;
        DeviceStatus.LastStatusText = "STATUS:RUNNING";
        NotifyStateChanged();

        var duration = 90 + (int)Math.Min(700m, Math.Abs(deltaMm) * 28m);
        SimulateDelay(duration);

        switch (normalizedAxis)
        {
            case "X":
                DeviceStatus.ReportedX = (DeviceStatus.ReportedX ?? _profile.XMinMm) + deltaMm;
                break;
            case "Y":
                DeviceStatus.ReportedY = (DeviceStatus.ReportedY ?? _profile.YMinMm) + deltaMm;
                break;
            case "Z":
                DeviceStatus.ReportedZ = (DeviceStatus.ReportedZ ?? _profile.ZMinMm) + deltaMm;
                break;
            default:
                throw new InvalidOperationException("Unsupported jog axis.");
        }

        DeviceStatus.HasReportedPosition = true;
        DeviceStatus.DeviceState = CncDeviceState.Idle;
        DeviceStatus.LastStatusText = $"POS:{DeviceStatus.ReportedX ?? 0m:0.###},{DeviceStatus.ReportedY ?? 0m:0.###},{DeviceStatus.ReportedZ ?? 0m:0.###}";
        DeviceStatus.LastAcknowledgement = DeviceStatus.LastStatusText;
        DeviceStatus.LastAcknowledgedAt = DateTime.Now;
        AddDriverLog($"Simulated jog {normalizedAxis} completed.", "Info");
        NotifyStateChanged();
        return DeviceStatus.LastStatusText;
    }

    public string MoveLinear(decimal deltaXmm, decimal deltaYmm, decimal deltaZmm)
    {
        EnsureConnected();
        EnsureMotorsEnabled();

        DeviceStatus.DeviceState = CncDeviceState.Running;
        DeviceStatus.LastStatusText = "STATUS:RUNNING";
        NotifyStateChanged();

        var distance = Math.Sqrt((double)((deltaXmm * deltaXmm) + (deltaYmm * deltaYmm) + (deltaZmm * deltaZmm)));
        var duration = 100 + (int)Math.Min(1400d, distance * 24d);
        SimulateDelay(duration);

        DeviceStatus.ReportedX = (DeviceStatus.ReportedX ?? _profile.XMinMm) + deltaXmm;
        DeviceStatus.ReportedY = (DeviceStatus.ReportedY ?? _profile.YMinMm) + deltaYmm;
        DeviceStatus.ReportedZ = (DeviceStatus.ReportedZ ?? _profile.ZMinMm) + deltaZmm;
        DeviceStatus.HasReportedPosition = true;
        DeviceStatus.DeviceState = CncDeviceState.Idle;
        DeviceStatus.LastStatusText = $"POS:{DeviceStatus.ReportedX ?? 0m:0.###},{DeviceStatus.ReportedY ?? 0m:0.###},{DeviceStatus.ReportedZ ?? 0m:0.###}";
        DeviceStatus.LastAcknowledgement = DeviceStatus.LastStatusText;
        DeviceStatus.LastAcknowledgedAt = DateTime.Now;
        AddDriverLog($"Simulated coordinated move completed. ΔX {deltaXmm:0.###} / ΔY {deltaYmm:0.###} / ΔZ {deltaZmm:0.###} mm.", "Info");
        NotifyStateChanged();
        return DeviceStatus.LastStatusText;
    }

    public CncControllerAckResult ExecutePlannedCommand(CncPlannedCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        var startedAt = DateTime.UtcNow;
        try
        {
            string response;
            if (command.CommandText.Equals("M3", StringComparison.OrdinalIgnoreCase))
            {
                response = Complete("SPINDLE ON", "SPINDLE:ON");
            }
            else if (command.CommandText.Equals("M5", StringComparison.OrdinalIgnoreCase))
            {
                response = Complete("SPINDLE OFF", "SPINDLE:OFF");
            }
            else if (command.CommandText.StartsWith("J", StringComparison.OrdinalIgnoreCase))
            {
                response = ExecuteSimulatedJog(command.CommandText);
            }
            else if (command.CommandText.StartsWith("G0", StringComparison.OrdinalIgnoreCase)
                     || command.CommandText.StartsWith("G1", StringComparison.OrdinalIgnoreCase))
            {
                var targetX = command.ExpectedEndX ?? (_profile.XMinMm + (decimal)(DeviceStatus.ReportedX ?? 0m));
                var targetY = command.ExpectedEndY ?? (_profile.YMinMm + (decimal)(DeviceStatus.ReportedY ?? 0m));
                var targetZ = command.ExpectedEndZ ?? (_profile.ZMinMm + (decimal)(DeviceStatus.ReportedZ ?? 0m));
                var currentX = DeviceStatus.ReportedX ?? _profile.XMinMm;
                var currentY = DeviceStatus.ReportedY ?? _profile.YMinMm;
                var currentZ = DeviceStatus.ReportedZ ?? _profile.ZMinMm;
                response = MoveLinear(targetX - currentX, targetY - currentY, targetZ - currentZ);
            }
            else if (command.CommandText.StartsWith("G2", StringComparison.OrdinalIgnoreCase)
                     || command.CommandText.StartsWith("G3", StringComparison.OrdinalIgnoreCase))
            {
                response = ExecuteSimulatedArc(command);
            }
            else
            {
                response = ExecuteLegacyCommand(command.CommandText);
            }

            return new CncControllerAckResult
            {
                Success = !IsErrorLikeText(response),
                IsAlarm = response.StartsWith("ALARM:", StringComparison.OrdinalIgnoreCase),
                ResponseText = response,
                ErrorMessage = IsErrorLikeText(response) ? response : null,
                AcknowledgedAt = DateTime.UtcNow,
                RoundTripMilliseconds = (DateTime.UtcNow - startedAt).TotalMilliseconds,
                SourceLineNumber = command.SourceLineNumber
            };
        }
        catch (Exception ex)
        {
            return new CncControllerAckResult
            {
                Success = false,
                ResponseText = ex.Message,
                ErrorMessage = ex.Message,
                AcknowledgedAt = DateTime.UtcNow,
                RoundTripMilliseconds = (DateTime.UtcNow - startedAt).TotalMilliseconds,
                SourceLineNumber = command.SourceLineNumber
            };
        }
    }

    private string Complete(string command, string response)
    {
        DeviceStatus.IsResponsive = true;
        DeviceStatus.IsReady = true;
        if (!response.StartsWith("ERR:", StringComparison.OrdinalIgnoreCase) && !response.StartsWith("ALARM:", StringComparison.OrdinalIgnoreCase))
            DeviceStatus.IsAlarmed = false;
        DeviceStatus.LastAcknowledgement = response;
        DeviceStatus.LastAcknowledgedAt = DateTime.Now;
        DeviceStatus.LastStatusText = response;
        AddProtocolLog($"Simulated {command} acknowledged with {response}.", "Info");
        NotifyStateChanged();
        return response;
    }

    private void ResetStatus()
    {
        DeviceStatus.IsResponsive = false;
        DeviceStatus.IsReady = false;
        DeviceStatus.IsLocked = false;
        DeviceStatus.IsAlarmed = false;
        DeviceStatus.DeviceState = CncDeviceState.Unknown;
        DeviceStatus.LastProtocolError = null;
        DeviceStatus.LastStatusText = null;
        DeviceStatus.LastAcknowledgement = null;
        DeviceStatus.LastAcknowledgedAt = null;
        DeviceStatus.FirmwareVersion = "SIM-1.0";
        DeviceStatus.ProtocolVersion = "SIMULATION";
        DeviceStatus.FirmwareIdentity = new CncFirmwareIdentity();
        DeviceStatus.FirmwareCompatibility = new CncFirmwareCompatibilityResult();
        DeviceStatus.LimitXTriggered = false;
        DeviceStatus.LimitYTriggered = false;
        DeviceStatus.LimitZTriggered = false;
    }

    private void EnsureConnected()
    {
        if (!_isConnected)
            throw new InvalidOperationException("Connect the simulated CNC driver first.");
    }

    private void EnsureMotorsEnabled()
    {
        if (!_motorsEnabled)
            throw new InvalidOperationException("Enable motors before sending simulated motion commands.");
    }

    private static string FormatStatus(CncDeviceState state)
    {
        return state switch
        {
            CncDeviceState.Ready => "IDLE",
            CncDeviceState.Idle => "IDLE",
            CncDeviceState.Running => "RUNNING",
            CncDeviceState.Homing => "HOMING",
            CncDeviceState.Paused => "PAUSED",
            CncDeviceState.Stopped => "STOPPED",
            CncDeviceState.Alarm => "ALARM",
            CncDeviceState.Error => "ERROR",
            CncDeviceState.LimitHit => "LIMIT HIT",
            CncDeviceState.Disconnected => "DISCONNECTED",
            _ => "UNKNOWN"
        };
    }

    private static void SimulateDelay(int milliseconds)
    {
        if (milliseconds > 0)
            Thread.Sleep(milliseconds);
    }

    private void AddDriverLog(string message, string status)
    {
        AddLog("Simulation", message, status);
        _loggingService.AddLog("CNC Simulation", message, status);
    }

    private void AddProtocolLog(string message, string status)
    {
        AddLog("Protocol", message, status);
        _loggingService.AddLog("CNC Protocol", message, status);
    }

    private void AddLog(string direction, string message, string status)
    {
        RunOnUiThread(() => SerialLogs.Insert(0, new LogEntry
        {
            Timestamp = DateTime.Now,
            Direction = direction,
            Message = message,
            Status = status
        }));
    }

    private void NotifyStateChanged()
    {
        RunOnUiThread(() => StateChanged?.Invoke(this, EventArgs.Empty));
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

    private string ExecuteSimulatedJog(string commandText)
    {
        var axis = commandText[1].ToString();
        if (!decimal.TryParse(commandText[2..], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var distance))
            throw new InvalidOperationException($"Unsupported simulated jog command '{commandText}'.");

        return Jog(axis, distance);
    }

    private string ExecuteLegacyCommand(string commandText)
    {
        if (commandText.StartsWith("XY,", StringComparison.OrdinalIgnoreCase))
        {
            var parts = commandText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 3
                || !int.TryParse(parts[1], out var signedX)
                || !int.TryParse(parts[2], out var signedY))
            {
                throw new InvalidOperationException($"Unsupported simulated command '{commandText}'.");
            }

            var deltaX = signedX / _profile.XStepsPerMm;
            var deltaY = signedY / _profile.YStepsPerMm;
            return MoveLinear(deltaX, deltaY, 0m);
        }

        if (commandText.Length >= 3 && (commandText[0] == '+' || commandText[0] == '-'))
        {
            var axis = commandText[^1].ToString();
            var stepText = commandText[1..^1];
            if (!int.TryParse(stepText, out var steps))
                throw new InvalidOperationException($"Unsupported simulated command '{commandText}'.");

            var stepsPerMm = axis.ToUpperInvariant() switch
            {
                "X" => _profile.XStepsPerMm,
                "Y" => _profile.YStepsPerMm,
                _ => _profile.ZStepsPerMm
            };

            var distance = steps / stepsPerMm;
            if (commandText[0] == '-')
                distance = -distance;

            return Jog(axis, distance);
        }

        throw new InvalidOperationException($"Unsupported simulated command '{commandText}'.");
    }

    private static bool IsErrorLikeText(string text)
    {
        return text.StartsWith("ERR:", StringComparison.OrdinalIgnoreCase)
               || text.StartsWith("ALARM:", StringComparison.OrdinalIgnoreCase);
    }

    private string ExecuteSimulatedArc(CncPlannedCommand command)
    {
        EnsureConnected();
        EnsureMotorsEnabled();

        DeviceStatus.DeviceState = CncDeviceState.Running;
        DeviceStatus.LastStatusText = "STATUS:RUNNING";
        NotifyStateChanged();

        var duration = 120 + (int)Math.Min(1600m, command.EstimatedDistanceMm * 24m);
        SimulateDelay(duration);

        DeviceStatus.ReportedX = command.ExpectedEndX ?? DeviceStatus.ReportedX;
        DeviceStatus.ReportedY = command.ExpectedEndY ?? DeviceStatus.ReportedY;
        DeviceStatus.ReportedZ = command.ExpectedEndZ ?? DeviceStatus.ReportedZ;
        DeviceStatus.HasReportedPosition = true;
        DeviceStatus.DeviceState = CncDeviceState.Idle;
        DeviceStatus.LastStatusText = $"POS:{DeviceStatus.ReportedX ?? 0m:0.###},{DeviceStatus.ReportedY ?? 0m:0.###},{DeviceStatus.ReportedZ ?? 0m:0.###}";
        DeviceStatus.LastAcknowledgement = DeviceStatus.LastStatusText;
        DeviceStatus.LastAcknowledgedAt = DateTime.Now;
        AddDriverLog($"Simulated arc completed with {command.EstimatedDistanceMm:0.###} mm of travel.", "Info");
        NotifyStateChanged();
        return DeviceStatus.LastStatusText;
    }
}
