using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Windows;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class ArduinoSerialCncDriver : ICncDriver
{
    private readonly ILoggingService _loggingService;
    private readonly CncProtocolCommandFormatter _commandFormatter = new();
    private readonly CncProtocolResponseParser _responseParser = new();
    private SerialPort? _serialPort;
    private string? _connectedPort;
    private bool _motorsEnabled;
    private CncMachineProfile _profile;

    public ArduinoSerialCncDriver(ILoggingService loggingService, CncMachineProfile profile)
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
            SupportsPause = false,
            SupportsAlarmReset = true,
            SupportsWorkCoordinateSystem = true,
            VisualizationModelType = "Gantry3Axis"
        };
    }

    public ObservableCollection<LogEntry> SerialLogs { get; } = new();
    public CncDriverType DriverType => CncDriverType.ArduinoSerial;
    public CncDriverCapabilities Capabilities { get; }
    public CncDeviceStatusSnapshot DeviceStatus { get; }
    public bool IsConnected => _serialPort?.IsOpen == true;
    public string? ConnectedPort => _connectedPort;
    public bool MotorsEnabled => _motorsEnabled;
    public event EventHandler? StateChanged;
    public event EventHandler? ConnectionLost;

    public IEnumerable<string> GetAvailablePorts() => SerialPort.GetPortNames().OrderBy(port => port).ToArray();

    public void ApplyProfile(CncMachineProfile profile)
    {
        _profile = profile;
    }

    public void Connect(string portName)
    {
        if (string.IsNullOrWhiteSpace(portName))
            throw new InvalidOperationException("Select a COM port first.");

        if (IsConnected)
            return;

        _serialPort = new SerialPort(portName.Trim(), _profile.BaudRate)
        {
            ReadTimeout = 900,
            WriteTimeout = 900,
            NewLine = "\n",
            DtrEnable = true,
            RtsEnable = true
        };

        _serialPort.ErrorReceived += OnSerialErrorReceived;
        _serialPort.Open();
        _serialPort.DiscardInBuffer();
        _serialPort.DiscardOutBuffer();
        _connectedPort = portName.Trim();
        _motorsEnabled = false;
        ResetDeviceStatusForConnection();
        AddSerialLog("System", $"Connected to {_connectedPort} @ {_profile.BaudRate}", "Info");
        _loggingService.AddLog("CNC", $"Connected to {_connectedPort} @ {_profile.BaudRate}", "Info");

        try
        {
            DeviceStatus.DeviceState = CncDeviceState.Ready;
            DeviceStatus.IsReady = true;
            SendProtocolCommand(_commandFormatter.CreateStatusCommand());
            DeviceStatus.IsResponsive = true;
            DeviceStatus.IsReady = true;
            AddProtocolLog($"Handshake completed with {DeviceStatus.LastAcknowledgement}.", "Info");
        }
        catch (Exception ex)
        {
            var message = $"Protocol handshake failed: {ex.Message}";
            AddProtocolLog(message, "Error");
            CloseSerialPort();
            _connectedPort = null;
            ResetDeviceStatusForDisconnect(message);
            throw new InvalidOperationException(message, ex);
        }
    }

    public void Disconnect()
    {
        CloseSerialPort();
        _connectedPort = null;
        _motorsEnabled = false;
        ResetDeviceStatusForDisconnect();
        AddSerialLog("System", "Disconnected.", "Info");
        NotifyStateChanged();
    }

    public string EnableMotors()
    {
        var response = SendProtocolCommand(_commandFormatter.CreateEnableCommand());
        _motorsEnabled = !response.IsError;
        NotifyStateChanged();
        return response.RawText;
    }

    public string DisableMotors()
    {
        var response = SendProtocolCommand(_commandFormatter.CreateDisableCommand());
        if (!response.IsError)
            _motorsEnabled = false;
        NotifyStateChanged();
        return response.RawText;
    }

    public string AutoHome() => SendProtocolCommand(_commandFormatter.CreateHomeCommand()).RawText;
    public string ResetAlarm() => SendProtocolCommand(_commandFormatter.CreateResetCommand()).RawText;
    public string Stop() => SendProtocolCommand(_commandFormatter.CreateStopCommand()).RawText;
    public string RefreshStatus() => SendProtocolCommand(_commandFormatter.CreateStatusCommand()).RawText;

    public string Jog(string axis, decimal deltaMm)
    {
        var normalizedAxis = (axis ?? string.Empty).Trim().ToUpperInvariant();
        var stepsPerMm = normalizedAxis switch
        {
            "X" => _profile.XStepsPerMm,
            "Y" => _profile.YStepsPerMm,
            _ => _profile.ZStepsPerMm
        };

        var steps = (int)Math.Round(Math.Abs(deltaMm * stepsPerMm), MidpointRounding.AwayFromZero);
        if (steps <= 0)
            throw new InvalidOperationException("Jog step is too small for the current steps/mm configuration.");

        return SendProtocolCommand(_commandFormatter.CreateJogCommand(normalizedAxis, steps, deltaMm >= 0m)).RawText;
    }

    private CncProtocolResponse SendProtocolCommand(CncProtocolCommandSpec spec)
    {
        EnsureConnected();

        Exception? lastFailure = null;
        foreach (var command in spec.Commands)
        {
            try
            {
                return SendProtocolCommandAttempt(spec, command);
            }
            catch (TimeoutException ex)
            {
                lastFailure = ex;
                AddProtocolLog($"{spec.Name} timed out waiting for acknowledgement for '{command}'.", "Warning");
            }
            catch (InvalidOperationException ex) when (spec.Commands.Count > 1)
            {
                lastFailure = ex;
                AddProtocolLog($"{spec.Name} fallback activated after '{command}' failed: {ex.Message}", "Warning");
            }
        }

        HandleProtocolFailure(lastFailure?.Message ?? $"{spec.Name} failed without a valid acknowledgement.");
        throw new InvalidOperationException(lastFailure?.Message ?? $"{spec.Name} failed without a valid acknowledgement.", lastFailure);
    }

    private CncProtocolResponse SendProtocolCommandAttempt(CncProtocolCommandSpec spec, string command)
    {
        AddSerialLog("Sent", command, "Info");
        _loggingService.AddLog("CNC Sent", command, "Info");

        try
        {
            _serialPort!.WriteLine(command);
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or UnauthorizedAccessException)
        {
            HandleUnexpectedDisconnect($"Serial connection lost while sending '{command}'.");
            throw new InvalidOperationException("Serial connection was lost.", ex);
        }

        var deadline = DateTime.UtcNow.AddMilliseconds(spec.TimeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            string rawLine;
            try
            {
                var remainingMs = Math.Max(80, (int)(deadline - DateTime.UtcNow).TotalMilliseconds);
                _serialPort!.ReadTimeout = remainingMs;
                rawLine = _serialPort.ReadLine()?.Trim() ?? string.Empty;
            }
            catch (TimeoutException)
            {
                break;
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or UnauthorizedAccessException)
            {
                HandleUnexpectedDisconnect($"Serial connection lost while waiting for '{spec.Name}' acknowledgement.");
                throw new InvalidOperationException("Serial connection was lost.", ex);
            }

            if (string.IsNullOrWhiteSpace(rawLine))
                continue;

            var status = rawLine.StartsWith("ERR:", StringComparison.OrdinalIgnoreCase) ? "Error" : "Info";
            AddSerialLog("Received", rawLine, status);
            _loggingService.AddLog("CNC Received", rawLine, status);

            var response = _responseParser.Parse(rawLine);
            ApplyParsedResponse(response);
            AddProtocolLog($"Parsed {response.MessageType}: {response.RawText}", response.IsError ? "Error" : "Info");

            if (response.IsError)
            {
                HandleProtocolFailure(response.Message ?? response.RawText);
                throw new InvalidOperationException(response.Message ?? response.RawText);
            }

            if (spec.TerminalMessageTypes.Contains(response.MessageType))
                return response;
        }

        throw new TimeoutException($"{spec.Name} timed out after {spec.TimeoutMs} ms.");
    }

    private void ApplyParsedResponse(CncProtocolResponse response)
    {
        DeviceStatus.LastAcknowledgement = response.RawText;
        DeviceStatus.LastAcknowledgedAt = DateTime.Now;

        switch (response.MessageType)
        {
            case CncProtocolMessageType.Acknowledgement:
                DeviceStatus.IsResponsive = true;
                DeviceStatus.IsReady = true;
                DeviceStatus.LastStatusText = response.Message;
                break;
            case CncProtocolMessageType.Ready:
                DeviceStatus.IsResponsive = true;
                DeviceStatus.IsReady = true;
                DeviceStatus.DeviceState = CncDeviceState.Ready;
                DeviceStatus.LastStatusText = "READY";
                break;
            case CncProtocolMessageType.HomeDone:
                DeviceStatus.IsResponsive = true;
                DeviceStatus.IsReady = true;
                DeviceStatus.DeviceState = CncDeviceState.Idle;
                DeviceStatus.LastStatusText = "HOME DONE";
                break;
            case CncProtocolMessageType.Stopped:
                DeviceStatus.IsResponsive = true;
                DeviceStatus.DeviceState = CncDeviceState.Stopped;
                DeviceStatus.LastStatusText = "STOPPED";
                break;
            case CncProtocolMessageType.LimitHit:
                DeviceStatus.IsResponsive = true;
                DeviceStatus.DeviceState = CncDeviceState.LimitHit;
                DeviceStatus.LastProtocolError = "Controller reported LIMIT HIT.";
                break;
            case CncProtocolMessageType.Error:
                DeviceStatus.IsResponsive = true;
                DeviceStatus.LastProtocolError = response.Message ?? response.RawText;
                DeviceStatus.DeviceState = CncDeviceState.Error;
                break;
            case CncProtocolMessageType.Status:
                DeviceStatus.IsResponsive = true;
                DeviceStatus.IsReady = true;
                DeviceStatus.DeviceState = response.DeviceState ?? DeviceStatus.DeviceState;
                DeviceStatus.LastStatusText = response.Message;
                break;
            case CncProtocolMessageType.Position:
                DeviceStatus.IsResponsive = true;
                DeviceStatus.HasReportedPosition = true;
                DeviceStatus.ReportedX = response.X;
                DeviceStatus.ReportedY = response.Y;
                DeviceStatus.ReportedZ = response.Z;
                break;
            default:
                DeviceStatus.LastStatusText = response.RawText;
                break;
        }

        NotifyStateChanged();
    }

    private void ResetDeviceStatusForConnection()
    {
        DeviceStatus.IsResponsive = false;
        DeviceStatus.IsReady = false;
        DeviceStatus.DeviceState = CncDeviceState.Unknown;
        DeviceStatus.HasReportedPosition = false;
        DeviceStatus.ReportedX = null;
        DeviceStatus.ReportedY = null;
        DeviceStatus.ReportedZ = null;
        DeviceStatus.LastAcknowledgement = null;
        DeviceStatus.LastAcknowledgedAt = null;
        DeviceStatus.LastProtocolError = null;
        DeviceStatus.LastStatusText = null;
    }

    private void ResetDeviceStatusForDisconnect(string? reason = null)
    {
        DeviceStatus.IsResponsive = false;
        DeviceStatus.IsReady = false;
        DeviceStatus.DeviceState = CncDeviceState.Disconnected;
        DeviceStatus.LastProtocolError = reason;
        DeviceStatus.LastStatusText = reason ?? "Disconnected";
    }

    private void HandleProtocolFailure(string message)
    {
        DeviceStatus.LastProtocolError = message;
        DeviceStatus.IsReady = false;
        DeviceStatus.DeviceState = CncDeviceState.Error;
        AddProtocolLog(message, "Error");
        NotifyStateChanged();
    }

    private void EnsureConnected()
    {
        if (!IsConnected || _serialPort == null)
            throw new InvalidOperationException("Connect the CNC machine first.");
    }

    private void HandleUnexpectedDisconnect(string reason)
    {
        CloseSerialPort();
        _connectedPort = null;
        _motorsEnabled = false;
        ResetDeviceStatusForDisconnect(reason);
        AddSerialLog("System", reason, "Error");
        _loggingService.AddLog("CNC", reason, "Error");
        RunOnUiThread(() => ConnectionLost?.Invoke(this, EventArgs.Empty));
        NotifyStateChanged();
    }

    private void OnSerialErrorReceived(object? sender, SerialErrorReceivedEventArgs e)
    {
        DeviceStatus.LastStatusText = $"Serial warning: {e.EventType}.";
        AddSerialLog("System", DeviceStatus.LastStatusText, "Warning");
        _loggingService.AddLog("CNC", DeviceStatus.LastStatusText, "Warning");
        NotifyStateChanged();
    }

    private void AddProtocolLog(string message, string status)
    {
        AddSerialLog("Protocol", message, status);
        _loggingService.AddLog("CNC Protocol", message, status);
    }

    private void NotifyStateChanged()
    {
        RunOnUiThread(() => StateChanged?.Invoke(this, EventArgs.Empty));
    }

    private void AddSerialLog(string direction, string message, string status)
    {
        RunOnUiThread(() => SerialLogs.Insert(0, new LogEntry
        {
            Timestamp = DateTime.Now,
            Direction = direction,
            Message = message,
            Status = status
        }));
    }

    private void CloseSerialPort()
    {
        if (_serialPort == null)
            return;

        try
        {
            _serialPort.ErrorReceived -= OnSerialErrorReceived;
        }
        catch
        {
            // Ignore event unsubscription issues on teardown.
        }

        if (_serialPort.IsOpen)
        {
            _serialPort.Close();
            _serialPort.Dispose();
        }

        _serialPort = null;
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
