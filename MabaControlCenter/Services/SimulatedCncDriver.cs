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
        DeviceStatus.DeviceState = CncDeviceState.Ready;
        DeviceStatus.LastAcknowledgement = "READY";
        DeviceStatus.LastAcknowledgedAt = DateTime.Now;
        DeviceStatus.LastStatusText = "STATUS:IDLE";
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
        DeviceStatus.DeviceState = CncDeviceState.Idle;
        return Complete("ENABLE", "OK");
    }

    public string DisableMotors()
    {
        EnsureConnected();
        SimulateDelay(120);
        _motorsEnabled = false;
        DeviceStatus.DeviceState = CncDeviceState.Idle;
        return Complete("DISABLE", "OK");
    }

    public string AutoHome()
    {
        EnsureConnected();
        EnsureMotorsEnabled();
        DeviceStatus.DeviceState = CncDeviceState.Homing;
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
        DeviceStatus.DeviceState = CncDeviceState.Idle;
        return Complete("RESET", "OK");
    }

    public string Stop()
    {
        EnsureConnected();
        SimulateDelay(80);
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

    private string Complete(string command, string response)
    {
        DeviceStatus.IsResponsive = true;
        DeviceStatus.IsReady = true;
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
        DeviceStatus.DeviceState = CncDeviceState.Unknown;
        DeviceStatus.LastProtocolError = null;
        DeviceStatus.LastStatusText = null;
        DeviceStatus.LastAcknowledgement = null;
        DeviceStatus.LastAcknowledgedAt = null;
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
}
