using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Windows;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class ArduinoSerialCncDriver : ICncDriver
{
    private enum DetectedFirmwareMode
    {
        Unknown,
        Maba,
        LegacySimple,
        Custom
    }

    private readonly ILoggingService _loggingService;
    private readonly CncProtocolCommandFormatter _commandFormatter = new();
    private readonly CncProtocolResponseParser _responseParser = new();
    private SerialPort? _serialPort;
    private string? _connectedPort;
    private bool _motorsEnabled;
    private CncMachineProfile _profile;
    private decimal _estimatedX;
    private decimal _estimatedY;
    private decimal _estimatedZ;
    private DetectedFirmwareMode _detectedFirmwareMode = DetectedFirmwareMode.Unknown;

    public ArduinoSerialCncDriver(ILoggingService loggingService, CncMachineProfile profile)
    {
        _loggingService = loggingService;
        _profile = profile;
        DeviceStatus = new CncDeviceStatusSnapshot();
        Capabilities = new CncDriverCapabilities
        {
            VisualizationModelType = "Gantry3Axis"
        };
        RefreshCapabilities();
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
        RefreshCapabilities();
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
        _detectedFirmwareMode = DetectedFirmwareMode.Unknown;
        ResetDeviceStatusForConnection();
        AddSerialLog("System", $"Connected to {_connectedPort} @ {_profile.BaudRate}", "Info");
        _loggingService.AddLog("CNC", $"Connected to {_connectedPort} @ {_profile.BaudRate}", "Info");

        try
        {
            Thread.Sleep(1800);
            var startupBanner = DrainAvailableText();
            _detectedFirmwareMode = DetectFirmwareMode(startupBanner);
            RefreshCapabilities();

            if (UsesMabaMotionFirmware)
            {
                DeviceStatus.DeviceState = CncDeviceState.Ready;
                DeviceStatus.IsResponsive = true;
                DeviceStatus.IsReady = true;
                DeviceStatus.IsLocked = true;
                DeviceStatus.IsAlarmed = false;
                DeviceStatus.LastAcknowledgement = string.IsNullOrWhiteSpace(startupBanner) ? "MABA CNC FIRMWARE READY" : startupBanner;
                DeviceStatus.LastStatusText = string.IsNullOrWhiteSpace(startupBanner)
                    ? "MABA CNC FIRMWARE READY"
                    : startupBanner;
                DeviceStatus.FirmwareVersion = "2.0.0";
                DeviceStatus.ProtocolVersion = "MabaProtocol";
                MarkVerifiedStatus();
                AddProtocolLog("MABA motion firmware connected.", "Info");
                RefreshStatus();
                UpdateFirmwareIdentityFromHandshake(TryEnhanceFirmwareIdentity(startupBanner), verified: false);
            }
            else if (UsesSimpleFirmwareProtocol)
            {
                DeviceStatus.DeviceState = CncDeviceState.Ready;
                DeviceStatus.IsResponsive = true;
                DeviceStatus.IsReady = true;
                DeviceStatus.IsLocked = false;
                DeviceStatus.IsAlarmed = false;
                DeviceStatus.LastAcknowledgement = string.IsNullOrWhiteSpace(startupBanner) ? "Ready" : startupBanner;
                DeviceStatus.LastStatusText = string.IsNullOrWhiteSpace(startupBanner)
                    ? "Simple firmware connected."
                    : startupBanner;
                DeviceStatus.ProtocolVersion = "LegacySimple";
                DeviceStatus.StatusConfidence = CncControllerStatusConfidence.LastKnown;
                AddProtocolLog("Simple firmware connected using legacy step-command mode.", "Info");
                UpdateFirmwareIdentityFromHandshake(startupBanner, verified: false);
            }
            else
            {
                DeviceStatus.DeviceState = CncDeviceState.Ready;
                DeviceStatus.IsReady = true;
                DeviceStatus.IsLocked = false;
                DeviceStatus.IsAlarmed = false;
                DeviceStatus.ProtocolVersion = "Custom";
                SendProtocolCommand(_commandFormatter.CreateStatusCommand());
                DeviceStatus.IsResponsive = true;
                DeviceStatus.IsReady = true;
                MarkVerifiedStatus();
                AddProtocolLog($"Handshake completed with {DeviceStatus.LastAcknowledgement}.", "Info");
                UpdateFirmwareIdentityFromHandshake(TryEnhanceFirmwareIdentity(DeviceStatus.LastAcknowledgement), verified: false);
            }
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
        _detectedFirmwareMode = DetectedFirmwareMode.Unknown;
        ResetDeviceStatusForDisconnect();
        AddSerialLog("System", "Disconnected.", "Info");
        NotifyStateChanged();
    }

    public string EnableMotors()
    {
        if (UsesMabaMotionFirmware)
        {
            var response = SendMabaCommandAwaiting("Unlock", "$X", new[] { "UNLOCKED", "OK" }, 5000);
            _motorsEnabled = !IsErrorLikeText(response);
            return response;
        }

        if (UsesSimpleFirmwareProtocol)
            return UnsupportedResponse("Enable Motors", "This firmware keeps the CNC shield enabled directly and does not support ENABLE.");

        var protocolResponse = SendProtocolCommand(_commandFormatter.CreateEnableCommand());
        _motorsEnabled = !protocolResponse.IsError;
        NotifyStateChanged();
        return protocolResponse.RawText;
    }

    public string DisableMotors()
    {
        if (UsesMabaMotionFirmware)
            return UnsupportedResponse("Disable Motors", "This firmware does not expose a dedicated motor-disable command.");

        if (UsesSimpleFirmwareProtocol)
            return UnsupportedResponse("Disable Motors", "This firmware does not support DISABLE.");

        var protocolResponse = SendProtocolCommand(_commandFormatter.CreateDisableCommand());
        if (!protocolResponse.IsError)
            _motorsEnabled = false;
        NotifyStateChanged();
        return protocolResponse.RawText;
    }

    public string AutoHome()
    {
        if (UsesMabaMotionFirmware)
        {
            DeviceStatus.DeviceState = CncDeviceState.Homing;
            DeviceStatus.IsReady = false;
            NotifyStateChanged();
            var response = SendMabaCommandAwaiting("Home", "$H", new[] { "HOME:DONE", "HOME DONE" }, 90000);
            _estimatedX = 0m;
            _estimatedY = 0m;
            _estimatedZ = 0m;
            DeviceStatus.ReportedX = 0m;
            DeviceStatus.ReportedY = 0m;
            DeviceStatus.ReportedZ = 0m;
            DeviceStatus.HasReportedPosition = true;
            _motorsEnabled = true;
            return response;
        }

        if (UsesSimpleFirmwareProtocol)
        {
            DeviceStatus.DeviceState = CncDeviceState.Homing;
            DeviceStatus.IsReady = false;
            NotifyStateChanged();
            return SendLegacyCommandAwaiting("Home", "H", new[] { "HOME:DONE", "HOME DONE" }, 90000);
        }

        return SendProtocolCommand(_commandFormatter.CreateHomeCommand()).RawText;
    }

    public string ResetAlarm()
    {
        if (UsesMabaMotionFirmware)
        {
            var response = SendMabaCommandAwaiting("Unlock", "$X", new[] { "UNLOCKED", "OK" }, 5000);
            _motorsEnabled = !IsErrorLikeText(response);
            return response;
        }

        if (UsesSimpleFirmwareProtocol)
            return UnsupportedResponse("Reset", "This firmware does not support alarm reset.");

        return SendProtocolCommand(_commandFormatter.CreateResetCommand()).RawText;
    }

    public string Stop()
    {
        if (UsesMabaMotionFirmware)
        {
            var response = SendMabaCommandAwaiting("Stop", "!", new[] { "ALARM:", "ERR:" }, 5000);
            _motorsEnabled = false;
            return response;
        }

        if (UsesSimpleFirmwareProtocol)
            return UnsupportedResponse("Stop", "This firmware does not support STOP.");

        return SendProtocolCommand(_commandFormatter.CreateStopCommand()).RawText;
    }

    public string RefreshStatus()
    {
        if (UsesMabaMotionFirmware)
            return SendMabaStatusCommand();

        if (UsesSimpleFirmwareProtocol)
        {
            DeviceStatus.IsResponsive = true;
            DeviceStatus.IsReady = true;
            MarkLastKnownStatus();
            DeviceStatus.DeviceState = CncDeviceState.Idle;
            DeviceStatus.LastStatusText = "Legacy firmware has no STATUS command.";
            NotifyStateChanged();
            return "STATUS:UNSUPPORTED";
        }

        return SendProtocolCommand(_commandFormatter.CreateStatusCommand()).RawText;
    }

    public string Jog(string axis, decimal deltaMm)
    {
        if (UsesMabaMotionFirmware)
        {
            var normalizedAxis = (axis ?? string.Empty).Trim().ToUpperInvariant();
            var firmwareDelta = ApplyAxisDirection(normalizedAxis, deltaMm);
            var magnitude = Math.Abs(firmwareDelta);
            if (magnitude <= 0m)
                throw new InvalidOperationException("Jog distance must be greater than zero.");

            var distanceText = magnitude.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            var command = normalizedAxis switch
            {
                "X" => $"JX{(firmwareDelta >= 0m ? distanceText : "-" + distanceText)}",
                "Y" => $"JY{(firmwareDelta >= 0m ? distanceText : "-" + distanceText)}",
                "Z" => $"JZ{(firmwareDelta >= 0m ? distanceText : "-" + distanceText)}",
                _ => throw new InvalidOperationException("Unsupported jog axis.")
            };

            var response = SendMabaCommandAwaiting("Jog", command, new[] { "OK" }, 15000);
            UpdateEstimatedPosition(
                normalizedAxis == "X" ? deltaMm : 0m,
                normalizedAxis == "Y" ? deltaMm : 0m,
                normalizedAxis == "Z" ? deltaMm : 0m);
            return response;
        }

        var legacyAxis = (axis ?? string.Empty).Trim().ToUpperInvariant();
        var legacyFirmwareDelta = ApplyAxisDirection(legacyAxis, deltaMm);
        var stepsPerMm = legacyAxis switch
        {
            "X" => _profile.XStepsPerMm,
            "Y" => _profile.YStepsPerMm,
            _ => _profile.ZStepsPerMm
        };

        var steps = (int)Math.Round(Math.Abs(legacyFirmwareDelta * stepsPerMm), MidpointRounding.AwayFromZero);
        if (steps <= 0)
            throw new InvalidOperationException("Jog step is too small for the current steps/mm configuration.");

        if (UsesSimpleFirmwareProtocol)
            return SendLegacyJogCommand(legacyAxis, steps, legacyFirmwareDelta >= 0m);

        return SendProtocolCommand(_commandFormatter.CreateJogCommand(legacyAxis, steps, legacyFirmwareDelta >= 0m)).RawText;
    }

    public string MoveLinear(decimal deltaXmm, decimal deltaYmm, decimal deltaZmm)
    {
        if (UsesMabaMotionFirmware)
            return SendMabaLinearMove(deltaXmm, deltaYmm, deltaZmm);

        if (UsesSimpleFirmwareProtocol)
            return SendLegacyLinearMove(deltaXmm, deltaYmm, deltaZmm);

        var responses = new List<string>();
        if (deltaXmm != 0m)
            responses.Add(Jog("X", deltaXmm));
        if (deltaYmm != 0m)
            responses.Add(Jog("Y", deltaYmm));
        if (deltaZmm != 0m)
            responses.Add(Jog("Z", deltaZmm));

        return string.Join(" | ", responses.Where(r => !string.IsNullOrWhiteSpace(r)));
    }

    public CncControllerAckResult ExecutePlannedCommand(CncPlannedCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        var startedAt = DateTime.UtcNow;
        try
        {
            var response = ExecuteCommandText(command.CommandText, command.RequiresAck);
            return new CncControllerAckResult
            {
                Success = !IsErrorLikeText(response),
                IsAlarm = response.StartsWith("ALARM:", StringComparison.OrdinalIgnoreCase),
                IsTimeout = false,
                ResponseText = response,
                ErrorMessage = IsErrorLikeText(response) ? response : null,
                AcknowledgedAt = DateTime.UtcNow,
                RoundTripMilliseconds = (DateTime.UtcNow - startedAt).TotalMilliseconds,
                SourceLineNumber = command.SourceLineNumber
            };
        }
        catch (TimeoutException ex)
        {
            return new CncControllerAckResult
            {
                Success = false,
                IsTimeout = true,
                ResponseText = ex.Message,
                ErrorMessage = ex.Message,
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
                IsAlarm = DeviceStatus.IsAlarmed,
                ResponseText = ex.Message,
                ErrorMessage = ex.Message,
                AcknowledgedAt = DateTime.UtcNow,
                RoundTripMilliseconds = (DateTime.UtcNow - startedAt).TotalMilliseconds,
                SourceLineNumber = command.SourceLineNumber
            };
        }
    }

    private string ExecuteCommandText(string commandText, bool requiresAck)
    {
        if (UsesMabaMotionFirmware)
            return SendMabaPlannerCommand(commandText, requiresAck);

        if (UsesSimpleFirmwareProtocol)
            return SendLegacyPlannerCommand(commandText, requiresAck);

        return SendCustomPlannerCommand(commandText);
    }

    private string SendMabaLinearMove(decimal deltaXmm, decimal deltaYmm, decimal deltaZmm)
    {
        var firmwareDeltaX = ApplyAxisDirection("X", deltaXmm);
        var firmwareDeltaY = ApplyAxisDirection("Y", deltaYmm);
        var firmwareDeltaZ = ApplyAxisDirection("Z", deltaZmm);

        var targetX = _estimatedX + firmwareDeltaX;
        var targetY = _estimatedY + firmwareDeltaY;
        var targetZ = _estimatedZ + firmwareDeltaZ;
        var command = string.Create(System.Globalization.CultureInfo.InvariantCulture, $"G1 X{targetX:0.###} Y{targetY:0.###} Z{targetZ:0.###} F300");
        var response = SendMabaCommandAwaiting("Linear Move", command, new[] { "OK" }, 30000);
        UpdateEstimatedPosition(deltaXmm, deltaYmm, deltaZmm);
        return response;
    }

    private string SendMabaPlannerCommand(string commandText, bool requiresAck)
    {
        if (!requiresAck)
        {
            EnsureConnected();
            AddSerialLog("Sent", commandText, "Info");
            _loggingService.AddLog("CNC Sent", commandText, "Info");
            _serialPort!.WriteLine(commandText);
            DeviceStatus.LastAcknowledgement = commandText;
            DeviceStatus.LastAcknowledgedAt = DateTime.Now;
            DeviceStatus.IsResponsive = true;
            DeviceStatus.LastStatusText = commandText;
            NotifyStateChanged();
            return commandText;
        }

        var expected = commandText.StartsWith("$H", StringComparison.OrdinalIgnoreCase) || commandText.Equals("H", StringComparison.OrdinalIgnoreCase)
            ? new[] { "HOME:DONE", "HOME DONE" }
            : commandText.Equals("$X", StringComparison.OrdinalIgnoreCase)
                ? new[] { "UNLOCKED", "OK" }
                : commandText.Equals("!", StringComparison.OrdinalIgnoreCase)
                    ? new[] { "ALARM:", "ERR:" }
                    : commandText.Equals("M3", StringComparison.OrdinalIgnoreCase)
                        ? new[] { "SPINDLE:ON" }
                        : commandText.Equals("M5", StringComparison.OrdinalIgnoreCase)
                            ? new[] { "SPINDLE:OFF" }
                    : new[] { "OK" };

        var timeoutMs = commandText.StartsWith("G", StringComparison.OrdinalIgnoreCase) ? 30000 : 15000;
        return SendMabaCommandAwaiting("Stream", commandText, expected, timeoutMs);
    }

    private string SendLegacyPlannerCommand(string commandText, bool requiresAck)
    {
        if (commandText.StartsWith("XY,", StringComparison.OrdinalIgnoreCase))
        {
            var parts = commandText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 3
                || !int.TryParse(parts[1], out var signedX)
                || !int.TryParse(parts[2], out var signedY))
            {
                throw new InvalidOperationException($"Unsupported legacy planner command '{commandText}'.");
            }

            return SendLegacyCombinedXyCommand(Math.Abs(signedX), signedX >= 0, Math.Abs(signedY), signedY >= 0);
        }

        if (commandText.Length >= 3 && (commandText[0] == '+' || commandText[0] == '-'))
        {
            var axis = commandText[^1].ToString().ToUpperInvariant();
            var stepText = commandText[1..^1];
            if (!int.TryParse(stepText, out var steps))
                throw new InvalidOperationException($"Unsupported legacy planner command '{commandText}'.");

            return SendLegacyJogCommand(axis, steps, commandText[0] == '+');
        }

        if (requiresAck)
            return SendLegacyCommandAwaiting("Legacy Stream", commandText, new[] { "OK" }, 15000);

        EnsureConnected();
        AddSerialLog("Sent", commandText, "Info");
        _loggingService.AddLog("CNC Sent", commandText, "Info");
        _serialPort!.WriteLine(commandText);
        Thread.Sleep(120);
        var residual = DrainAvailableText();
        if (!string.IsNullOrWhiteSpace(residual))
            AddSerialLog("Received", residual, "Info");
        DeviceStatus.LastAcknowledgement = residual ?? commandText;
        DeviceStatus.LastAcknowledgedAt = DateTime.Now;
        DeviceStatus.IsResponsive = true;
        DeviceStatus.LastStatusText = residual ?? commandText;
        NotifyStateChanged();
        return residual ?? commandText;
    }

    private string SendCustomPlannerCommand(string commandText)
    {
        var response = SendProtocolCommand(new CncProtocolCommandSpec
        {
            Name = "Planned Command",
            Commands = new[] { commandText },
            TerminalMessageTypes = new[] { CncProtocolMessageType.Acknowledgement, CncProtocolMessageType.Status, CncProtocolMessageType.Position },
            TimeoutMs = 15000,
            AllowLegacyStatusWords = true
        });
        return response.RawText;
    }

    private string SendLegacyLinearMove(decimal deltaXmm, decimal deltaYmm, decimal deltaZmm)
    {
        var adjustedX = ApplyAxisDirection("X", deltaXmm);
        var adjustedY = ApplyAxisDirection("Y", deltaYmm);
        var adjustedZ = ApplyAxisDirection("Z", deltaZmm);

        var xSteps = ToSteps(adjustedX, _profile.XStepsPerMm);
        var ySteps = ToSteps(adjustedY, _profile.YStepsPerMm);
        var zSteps = ToSteps(adjustedZ, _profile.ZStepsPerMm);

        var responses = new List<string>();
        if (xSteps > 0 && ySteps > 0)
            responses.Add(SendLegacyCombinedXyCommand(xSteps, adjustedX >= 0m, ySteps, adjustedY >= 0m));
        else
        {
            if (xSteps > 0)
                responses.Add(SendLegacyJogCommand("X", xSteps, adjustedX >= 0m));
            if (ySteps > 0)
                responses.Add(SendLegacyJogCommand("Y", ySteps, adjustedY >= 0m));
        }

        if (zSteps > 0)
            responses.Add(SendLegacyJogCommand("Z", zSteps, adjustedZ >= 0m));

        return string.Join(" | ", responses.Where(r => !string.IsNullOrWhiteSpace(r)));
    }

    private string SendLegacyJogCommand(string axis, int steps, bool positive)
    {
        EnsureConnected();
        var command = $"{(positive ? "+" : "-")}{steps}{axis.ToLowerInvariant()}";
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

        Thread.Sleep(CalculateLegacyMoveDelayMs(steps));
        var residual = DrainAvailableText();
        if (!string.IsNullOrWhiteSpace(residual))
            AddSerialLog("Received", residual, "Info");

        DeviceStatus.IsResponsive = true;
        DeviceStatus.IsReady = true;
        DeviceStatus.DeviceState = CncDeviceState.Idle;
        DeviceStatus.LastAcknowledgement = command;
        DeviceStatus.LastAcknowledgedAt = DateTime.Now;
        DeviceStatus.LastStatusText = "Legacy move completed.";
        NotifyStateChanged();
        return command;
    }

    private string SendLegacyCombinedXyCommand(int xSteps, bool positiveX, int ySteps, bool positiveY)
    {
        EnsureConnected();
        var signedX = positiveX ? xSteps : -xSteps;
        var signedY = positiveY ? ySteps : -ySteps;
        var command = $"XY,{signedX},{signedY}";
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

        Thread.Sleep(CalculateLegacyCombinedMoveDelayMs(xSteps, ySteps));
        var residual = DrainAvailableText();
        if (!string.IsNullOrWhiteSpace(residual))
            AddSerialLog("Received", residual, "Info");

        DeviceStatus.IsResponsive = true;
        DeviceStatus.IsReady = true;
        DeviceStatus.DeviceState = CncDeviceState.Idle;
        DeviceStatus.LastAcknowledgement = command;
        DeviceStatus.LastAcknowledgedAt = DateTime.Now;
        DeviceStatus.LastStatusText = "Legacy coordinated XY move completed.";
        NotifyStateChanged();
        return command;
    }

    private string SendLegacyCommandAwaiting(string name, string command, IReadOnlyCollection<string> expectedTokens, int timeoutMs)
    {
        EnsureConnected();
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

        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                _serialPort!.ReadTimeout = Math.Max(120, (int)(deadline - DateTime.UtcNow).TotalMilliseconds);
                var rawLine = _serialPort.ReadLine()?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(rawLine))
                    continue;

                AddSerialLog("Received", rawLine, "Info");
                _loggingService.AddLog("CNC Received", rawLine, "Info");

                if (expectedTokens.Any(token => rawLine.Contains(token, StringComparison.OrdinalIgnoreCase)))
                {
                    DeviceStatus.IsResponsive = true;
                    DeviceStatus.IsReady = true;
                    DeviceStatus.DeviceState = CncDeviceState.Idle;
                    DeviceStatus.LastAcknowledgement = rawLine;
                    DeviceStatus.LastAcknowledgedAt = DateTime.Now;
                    DeviceStatus.LastStatusText = rawLine;
                    AddProtocolLog($"{name} completed using legacy firmware acknowledgement.", "Info");
                    NotifyStateChanged();
                    return rawLine;
                }
            }
            catch (TimeoutException)
            {
                break;
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or UnauthorizedAccessException)
            {
                HandleUnexpectedDisconnect($"Serial connection lost while waiting for '{name}'.");
                throw new InvalidOperationException("Serial connection was lost.", ex);
            }
        }

        HandleProtocolFailure($"{name} timed out waiting for {string.Join(" or ", expectedTokens)}.");
        throw new TimeoutException($"{name} timed out after {timeoutMs} ms.");
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
        DeviceStatus.IsLocked = false;
        DeviceStatus.IsAlarmed = false;
        DeviceStatus.StatusConfidence = CncControllerStatusConfidence.Unknown;
        DeviceStatus.DeviceState = CncDeviceState.Unknown;
        DeviceStatus.HasReportedPosition = false;
        DeviceStatus.ReportedX = null;
        DeviceStatus.ReportedY = null;
        DeviceStatus.ReportedZ = null;
        DeviceStatus.LimitXTriggered = false;
        DeviceStatus.LimitYTriggered = false;
        DeviceStatus.LimitZTriggered = false;
        DeviceStatus.LastAcknowledgement = null;
        DeviceStatus.LastAcknowledgedAt = null;
        DeviceStatus.LastVerifiedStatusAt = null;
        DeviceStatus.LastProtocolError = null;
        DeviceStatus.LastStatusText = null;
        DeviceStatus.FirmwareIdentity = new CncFirmwareIdentity();
        DeviceStatus.FirmwareCompatibility = new CncFirmwareCompatibilityResult();
    }

    private void ResetDeviceStatusForDisconnect(string? reason = null)
    {
        DeviceStatus.IsResponsive = false;
        DeviceStatus.IsReady = false;
        DeviceStatus.IsLocked = false;
        DeviceStatus.IsAlarmed = false;
        DeviceStatus.StatusConfidence = CncControllerStatusConfidence.Unknown;
        DeviceStatus.DeviceState = CncDeviceState.Disconnected;
        DeviceStatus.LastVerifiedStatusAt = null;
        DeviceStatus.LastProtocolError = reason;
        DeviceStatus.LastStatusText = reason ?? "Disconnected";
        DeviceStatus.FirmwareIdentity = new CncFirmwareIdentity();
        DeviceStatus.FirmwareCompatibility = new CncFirmwareCompatibilityResult();
    }

    private void HandleProtocolFailure(string message)
    {
        DeviceStatus.LastProtocolError = message;
        DeviceStatus.IsReady = false;
        DeviceStatus.IsAlarmed = true;
        DeviceStatus.StatusConfidence = CncControllerStatusConfidence.Stale;
        DeviceStatus.DeviceState = CncDeviceState.Error;
        AddProtocolLog(message, "Error");
        NotifyStateChanged();
    }

    private void MarkVerifiedStatus()
    {
        DeviceStatus.StatusConfidence = CncControllerStatusConfidence.VerifiedFresh;
        DeviceStatus.LastVerifiedStatusAt = DateTime.UtcNow;
    }

    private void MarkLastKnownStatus()
    {
        DeviceStatus.StatusConfidence = CncControllerStatusConfidence.LastKnown;
    }

    private void MarkStaleStatus()
    {
        DeviceStatus.StatusConfidence = CncControllerStatusConfidence.Stale;
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

    private string? DrainAvailableText()
    {
        try
        {
            if (_serialPort == null || !_serialPort.IsOpen || _serialPort.BytesToRead <= 0)
                return null;

            var text = _serialPort.ReadExisting();
            return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        }
        catch
        {
            return null;
        }
    }

    private string SendMabaStatusCommand()
    {
        EnsureConnected();
        const string command = "?";
        AddSerialLog("Sent", command, "Info");
        _loggingService.AddLog("CNC Sent", command, "Info");

        try
        {
            _serialPort!.WriteLine(command);
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or UnauthorizedAccessException)
        {
            HandleUnexpectedDisconnect("Serial connection lost while sending status query.");
            throw new InvalidOperationException("Serial connection was lost.", ex);
        }

        var deadline = DateTime.UtcNow.AddMilliseconds(5000);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                _serialPort!.ReadTimeout = Math.Max(120, (int)(deadline - DateTime.UtcNow).TotalMilliseconds);
                var rawLine = _serialPort.ReadLine()?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(rawLine))
                    continue;

                AddSerialLog("Received", rawLine, rawLine.StartsWith("ERR:", StringComparison.OrdinalIgnoreCase) || rawLine.StartsWith("ALARM:", StringComparison.OrdinalIgnoreCase) ? "Error" : "Info");
                _loggingService.AddLog("CNC Received", rawLine, "Info");

                if (rawLine.StartsWith("<MABA:", StringComparison.OrdinalIgnoreCase))
                {
                    ApplyMabaStatusLine(rawLine);
                    DeviceStatus.LastAcknowledgement = rawLine;
                    DeviceStatus.LastAcknowledgedAt = DateTime.Now;
                    DeviceStatus.LastStatusText = rawLine;
                    MarkVerifiedStatus();
                    NotifyStateChanged();
                    return rawLine;
                }

                if (rawLine.StartsWith("ERR:", StringComparison.OrdinalIgnoreCase) || rawLine.StartsWith("ALARM:", StringComparison.OrdinalIgnoreCase))
                {
                    if (rawLine.StartsWith("ERR:", StringComparison.OrdinalIgnoreCase)
                        && rawLine.Contains("UNKNOWN_COMMAND", StringComparison.OrdinalIgnoreCase))
                    {
                        DeviceStatus.IsResponsive = true;
                        DeviceStatus.DeviceState = DeviceStatus.IsLocked ? CncDeviceState.Idle : CncDeviceState.Ready;
                        DeviceStatus.LastProtocolError = rawLine;
                        DeviceStatus.LastStatusText = "Firmware did not provide a status report. Using the last known controller state.";
                        MarkLastKnownStatus();
                        AddProtocolLog("Status query is unsupported by the connected firmware. Keeping the last known controller state.", "Warning");
                        NotifyStateChanged();
                        return rawLine;
                    }

                    DeviceStatus.LastProtocolError = rawLine;
                    DeviceStatus.IsAlarmed = rawLine.StartsWith("ALARM:", StringComparison.OrdinalIgnoreCase);
                    MarkVerifiedStatus();
                    DeviceStatus.DeviceState = rawLine.StartsWith("ALARM:", StringComparison.OrdinalIgnoreCase)
                        ? CncDeviceState.Alarm
                        : CncDeviceState.Error;
                    NotifyStateChanged();
                    return rawLine;
                }
            }
            catch (TimeoutException)
            {
                break;
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or UnauthorizedAccessException)
            {
                HandleUnexpectedDisconnect("Serial connection lost while waiting for status.");
                throw new InvalidOperationException("Serial connection was lost.", ex);
            }
        }

        DeviceStatus.IsResponsive = true;
        DeviceStatus.LastProtocolError = "Status timed out waiting for <MABA:...> response.";
        DeviceStatus.LastStatusText = "Status query timed out. Controller status is now stale until refreshed.";
        MarkStaleStatus();
        if (DeviceStatus.DeviceState == CncDeviceState.Unknown)
            DeviceStatus.DeviceState = DeviceStatus.IsLocked ? CncDeviceState.Idle : CncDeviceState.Ready;
        AddProtocolLog("Status query timed out. Dangerous actions are now blocked until status is refreshed.", "Warning");
        NotifyStateChanged();
        return "STATUS:TIMEOUT";
    }

    private string SendMabaCommandAwaiting(string name, string command, IReadOnlyCollection<string> expectedTokens, int timeoutMs)
    {
        EnsureConnected();
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

        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                _serialPort!.ReadTimeout = Math.Max(120, (int)(deadline - DateTime.UtcNow).TotalMilliseconds);
                var rawLine = _serialPort.ReadLine()?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(rawLine))
                    continue;

                var status = rawLine.StartsWith("ERR:", StringComparison.OrdinalIgnoreCase) || rawLine.StartsWith("ALARM:", StringComparison.OrdinalIgnoreCase)
                    ? "Error"
                    : "Info";
                AddSerialLog("Received", rawLine, status);
                _loggingService.AddLog("CNC Received", rawLine, status);

                if (rawLine.StartsWith("<MABA:", StringComparison.OrdinalIgnoreCase))
                {
                    ApplyMabaStatusLine(rawLine);
                    continue;
                }

                DeviceStatus.LastAcknowledgement = rawLine;
                DeviceStatus.LastAcknowledgedAt = DateTime.Now;
                DeviceStatus.LastStatusText = rawLine;
                DeviceStatus.IsResponsive = true;
                MarkVerifiedStatus();

                if (rawLine.StartsWith("ERR:", StringComparison.OrdinalIgnoreCase))
                {
                    DeviceStatus.LastProtocolError = rawLine;
                    DeviceStatus.IsAlarmed = false;
                    DeviceStatus.DeviceState = CncDeviceState.Error;
                    NotifyStateChanged();
                    return rawLine;
                }

                if (rawLine.StartsWith("ALARM:", StringComparison.OrdinalIgnoreCase))
                {
                    DeviceStatus.LastProtocolError = rawLine;
                    DeviceStatus.IsAlarmed = true;
                    DeviceStatus.IsLocked = true;
                    DeviceStatus.DeviceState = CncDeviceState.Alarm;
                    _motorsEnabled = false;
                    NotifyStateChanged();
                    return rawLine;
                }

                if (rawLine.Equals("UNLOCKED", StringComparison.OrdinalIgnoreCase))
                {
                    DeviceStatus.IsReady = true;
                    DeviceStatus.IsLocked = false;
                    DeviceStatus.IsAlarmed = false;
                    DeviceStatus.DeviceState = CncDeviceState.Ready;
                    _motorsEnabled = true;
                }
                else if (rawLine.Equals("HOME:DONE", StringComparison.OrdinalIgnoreCase))
                {
                    DeviceStatus.IsReady = true;
                    DeviceStatus.IsLocked = false;
                    DeviceStatus.IsAlarmed = false;
                    DeviceStatus.DeviceState = CncDeviceState.Idle;
                    _motorsEnabled = true;
                }
                else if (rawLine.Equals("OK", StringComparison.OrdinalIgnoreCase))
                {
                    DeviceStatus.IsReady = true;
                    DeviceStatus.IsAlarmed = false;
                    DeviceStatus.DeviceState = CncDeviceState.Idle;
                }

                NotifyStateChanged();

                if (expectedTokens.Any(token => rawLine.Contains(token, StringComparison.OrdinalIgnoreCase)))
                    return rawLine;
            }
            catch (TimeoutException)
            {
                break;
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or UnauthorizedAccessException)
            {
                HandleUnexpectedDisconnect($"Serial connection lost while waiting for '{name}'.");
                throw new InvalidOperationException("Serial connection was lost.", ex);
            }
        }

        HandleProtocolFailure($"{name} timed out waiting for firmware acknowledgement.");
        throw new TimeoutException($"{name} timed out waiting for firmware acknowledgement.");
    }

    private void ApplyMabaStatusLine(string rawLine)
    {
        var payload = rawLine.Trim().TrimStart('<').TrimEnd('>');
        var segments = payload.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var segment in segments)
        {
            var colon = segment.IndexOf(':');
            if (colon < 0)
                continue;

            values[segment[..colon]] = segment[(colon + 1)..];
        }

        var stateToken = values.TryGetValue("MABA", out var stateValue) ? stateValue : "LOCKED";
        var alarmActive = values.TryGetValue("ALARM", out var alarmValue) && alarmValue == "1";
        var xLimitTriggered = values.TryGetValue("XLIM", out var xLimitValue) && xLimitValue == "1";
        var yLimitTriggered = values.TryGetValue("YLIM", out var yLimitValue) && yLimitValue == "1";
        var x = values.TryGetValue("X", out var xValue) && decimal.TryParse(xValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsedX) ? parsedX : _estimatedX;
        var y = values.TryGetValue("Y", out var yValue) && decimal.TryParse(yValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsedY) ? parsedY : _estimatedY;
        var z = values.TryGetValue("Z", out var zValue) && decimal.TryParse(zValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsedZ) ? parsedZ : _estimatedZ;

        _estimatedX = ApplyAxisDirection("X", x);
        _estimatedY = ApplyAxisDirection("Y", y);
        _estimatedZ = ApplyAxisDirection("Z", z);

        DeviceStatus.ReportedX = _estimatedX;
        DeviceStatus.ReportedY = _estimatedY;
        DeviceStatus.ReportedZ = _estimatedZ;
        DeviceStatus.HasReportedPosition = true;
        DeviceStatus.IsResponsive = true;
        DeviceStatus.LastStatusText = rawLine;
        DeviceStatus.LimitXTriggered = xLimitTriggered;
        DeviceStatus.LimitYTriggered = yLimitTriggered;
        DeviceStatus.LastProtocolError = alarmActive ? "ALARM:LIMIT_OR_ESTOP" : null;
        DeviceStatus.IsAlarmed = alarmActive;
        DeviceStatus.IsLocked = !stateToken.Equals("READY", StringComparison.OrdinalIgnoreCase);
        UpdateFirmwareIdentityFromHandshake(DeviceStatus.LastAcknowledgement ?? rawLine, verified: false);

        if (alarmActive)
        {
            DeviceStatus.DeviceState = CncDeviceState.Alarm;
            DeviceStatus.IsReady = false;
            _motorsEnabled = false;
            return;
        }

        if (stateToken.Equals("READY", StringComparison.OrdinalIgnoreCase))
        {
            DeviceStatus.DeviceState = CncDeviceState.Ready;
            DeviceStatus.IsReady = true;
            DeviceStatus.IsLocked = false;
            _motorsEnabled = true;
            return;
        }

        DeviceStatus.DeviceState = CncDeviceState.Idle;
        DeviceStatus.IsReady = false;
        DeviceStatus.IsLocked = true;
        _motorsEnabled = false;
    }

    private bool UsesSimpleFirmwareProtocol
    {
        get
        {
            if (_detectedFirmwareMode == DetectedFirmwareMode.LegacySimple)
                return true;
            if (_detectedFirmwareMode == DetectedFirmwareMode.Maba)
                return false;

            var definition = _profile.DefinitionSnapshot;
            if (definition == null)
                return false;

            if (definition.RuntimeBinding.FirmwareProtocol == FirmwareProtocol.MabaProtocol)
                return false;

            if (definition.RuntimeBinding.FirmwareProtocol == FirmwareProtocol.Custom)
                return true;

            var protocol = definition.Capabilities.Protocol;
            return !protocol.StatusQuery
                   && !protocol.MotorEnable
                   && !protocol.MotorDisable
                   && !protocol.Acknowledgements;
        }
    }

    private bool UsesMabaMotionFirmware
    {
        get
        {
            if (_detectedFirmwareMode == DetectedFirmwareMode.Maba)
                return true;
            if (_detectedFirmwareMode == DetectedFirmwareMode.LegacySimple)
                return false;

            return _profile.DefinitionSnapshot?.RuntimeBinding.FirmwareProtocol == FirmwareProtocol.MabaProtocol;
        }
    }

    private static DetectedFirmwareMode DetectFirmwareMode(string? startupBanner)
    {
        if (string.IsNullOrWhiteSpace(startupBanner))
            return DetectedFirmwareMode.Unknown;

        if (startupBanner.Contains("MABA CNC FIRMWARE READY", StringComparison.OrdinalIgnoreCase)
            || startupBanner.Contains("LOCKED: SEND $H", StringComparison.OrdinalIgnoreCase)
            || startupBanner.Contains("<MABA:", StringComparison.OrdinalIgnoreCase))
        {
            return DetectedFirmwareMode.Maba;
        }

        if (startupBanner.Contains("Ready: +100x", StringComparison.OrdinalIgnoreCase)
            || startupBanner.Contains("HOME DONE", StringComparison.OrdinalIgnoreCase))
        {
            return DetectedFirmwareMode.LegacySimple;
        }

        return DetectedFirmwareMode.Custom;
    }

    private static int CalculateLegacyMoveDelayMs(int steps)
    {
        var estimated = 80 + (steps * 0.6);
        return (int)Math.Clamp(Math.Ceiling(estimated), 80, 10000);
    }

    private static int CalculateLegacyCombinedMoveDelayMs(int xSteps, int ySteps)
    {
        var maxSteps = Math.Max(xSteps, ySteps);
        var estimated = 100 + (maxSteps * 0.65);
        return (int)Math.Clamp(Math.Ceiling(estimated), 100, 12000);
    }

    private static int ToSteps(decimal deltaMm, decimal stepsPerMm)
    {
        return (int)Math.Round(Math.Abs(deltaMm * stepsPerMm), MidpointRounding.AwayFromZero);
    }

    private string UnsupportedResponse(string name, string message)
    {
        DeviceStatus.LastProtocolError = message;
        DeviceStatus.LastStatusText = message;
        AddProtocolLog($"{name} blocked: {message}", "Warning");
        NotifyStateChanged();
        return $"ERR:{message}";
    }

    private decimal ApplyAxisDirection(string axis, decimal deltaMm)
    {
        var definition = _profile.DefinitionSnapshot;
        if (definition?.AxisConfig.AxisDirections == null)
            return deltaMm;

        if (definition.AxisConfig.AxisDirections.TryGetValue(axis, out var direction) && direction == Direction.Inverted)
            return -deltaMm;

        return deltaMm;
    }

    private void UpdateEstimatedPosition(decimal deltaXmm, decimal deltaYmm, decimal deltaZmm)
    {
        _estimatedX += deltaXmm;
        _estimatedY += deltaYmm;
        _estimatedZ += deltaZmm;
        DeviceStatus.ReportedX = _estimatedX;
        DeviceStatus.ReportedY = _estimatedY;
        DeviceStatus.ReportedZ = _estimatedZ;
        DeviceStatus.HasReportedPosition = true;
        DeviceStatus.LastAcknowledgedAt = DateTime.Now;
        DeviceStatus.IsResponsive = true;
        DeviceStatus.IsReady = true;
        DeviceStatus.IsLocked = false;
        DeviceStatus.IsAlarmed = false;
        DeviceStatus.DeviceState = CncDeviceState.Idle;
        DeviceStatus.LastProtocolError = null;
        NotifyStateChanged();
    }

    private static bool IsErrorLikeText(string text)
    {
        return text.StartsWith("ERR:", StringComparison.OrdinalIgnoreCase)
               || text.StartsWith("ALARM:", StringComparison.OrdinalIgnoreCase);
    }

    private void RefreshCapabilities()
    {
        Capabilities.SupportsZHoming = false;
        Capabilities.SupportsCombinedXyMove = false;
        Capabilities.SupportsWorkCoordinateSystem = true;
        Capabilities.VisualizationModelType = "Gantry3Axis";

        if (UsesMabaMotionFirmware)
        {
            Capabilities.SupportsAcknowledgements = true;
            Capabilities.SupportsLivePositionReporting = true;
            Capabilities.SupportsCombinedXyMove = true;
            Capabilities.SupportsPause = false;
            Capabilities.SupportsAlarmReset = true;
            return;
        }

        if (UsesSimpleFirmwareProtocol)
        {
            Capabilities.SupportsAcknowledgements = false;
            Capabilities.SupportsLivePositionReporting = false;
            Capabilities.SupportsPause = false;
            Capabilities.SupportsAlarmReset = false;
            Capabilities.SupportsCombinedXyMove = true;
            return;
        }

        Capabilities.SupportsAcknowledgements = true;
        Capabilities.SupportsLivePositionReporting = true;
        Capabilities.SupportsCombinedXyMove = false;
        Capabilities.SupportsPause = false;
        Capabilities.SupportsAlarmReset = true;
    }

    private void UpdateFirmwareIdentityFromHandshake(string? startupBanner, bool verified)
    {
        var hasIdentityPayload = !string.IsNullOrWhiteSpace(startupBanner)
                                 && (startupBanner.Contains("$I", StringComparison.OrdinalIgnoreCase)
                                     || startupBanner.Contains("$VER", StringComparison.OrdinalIgnoreCase)
                                     || startupBanner.Contains("$CAPS", StringComparison.OrdinalIgnoreCase));
        var identity = UsesMabaMotionFirmware
            ? CreateMabaFirmwareIdentity(startupBanner, verified || hasIdentityPayload)
            : UsesSimpleFirmwareProtocol
                ? CreateLegacyFirmwareIdentity(startupBanner)
                : CreateGenericFirmwareIdentity(startupBanner);

        DeviceStatus.FirmwareIdentity = identity;
        DeviceStatus.FirmwareVersion = identity.FirmwareVersion;
        DeviceStatus.ProtocolVersion = identity.ProtocolVersion.RawVersion;
    }

    private CncFirmwareIdentity CreateMabaFirmwareIdentity(string? startupBanner, bool verified)
    {
        var definition = _profile.DefinitionSnapshot;
        var axes = definition?.AxisConfig.SupportedAxes.Select(axis => axis.ToString()).ToList()
                   ?? new List<string> { "X", "Y", "Z" };
        var identity = new CncFirmwareIdentity
        {
            FirmwareName = "MABA CNC Motion Firmware",
            FirmwareVersion = "2.0.0",
            ProtocolName = "MABA",
            ProtocolVersion = new CncProtocolVersion
            {
                ProtocolName = "MABA",
                RawVersion = "MabaProtocol",
                Major = 2,
                Minor = 0
            },
            Confidence = verified ? CncCapabilityConfidence.Verified : CncCapabilityConfidence.Inferred,
            Capabilities = new CncFirmwareCapabilities
            {
                SupportsStatusQuery = true,
                SupportsUnlock = true,
                SupportsMotorEnable = false,
                SupportsMotorDisable = false,
                SupportsHoming = true,
                SupportsJog = true,
                SupportsG0G1 = true,
                SupportsG2G3 = true,
                SupportsSpindleOnOff = true,
                SupportsSpindleSpeed = false,
                SupportsFeedHold = false,
                SupportsSoftwareStop = true,
                SupportsWorkOffsets = false,
                SupportsLimitReporting = true,
                SupportsPositionReporting = true,
                SupportsFirmwareUpload = true,
                SupportedAxes = axes,
                WorkspaceLimitX = _profile.XLimitMm,
                WorkspaceLimitY = _profile.YLimitMm,
                WorkspaceLimitZ = _profile.ZLimitMm
            }
        };

        if (!verified)
            identity.FirmwareWarnings.Add("Firmware capabilities inferred from startup banner and MABA status behavior.");

        if (!string.IsNullOrWhiteSpace(startupBanner) && !startupBanner.Contains("MABA", StringComparison.OrdinalIgnoreCase))
            identity.FirmwareWarnings.Add("Startup banner did not explicitly identify the firmware as MABA.");

        ApplyFutureIdentityPayloads(startupBanner, identity);

        return identity;
    }

    private CncFirmwareIdentity CreateLegacyFirmwareIdentity(string? startupBanner)
    {
        var identity = new CncFirmwareIdentity
        {
            FirmwareName = "Legacy Step Firmware",
            FirmwareVersion = "Unknown",
            ProtocolName = "LegacySimple",
            ProtocolVersion = new CncProtocolVersion
            {
                ProtocolName = "LegacySimple",
                RawVersion = "LegacySimple"
            },
            Confidence = CncCapabilityConfidence.Inferred,
            Capabilities = new CncFirmwareCapabilities
            {
                SupportsStatusQuery = false,
                SupportsUnlock = false,
                SupportsMotorEnable = false,
                SupportsMotorDisable = false,
                SupportsHoming = true,
                SupportsJog = true,
                SupportsG0G1 = false,
                SupportsG2G3 = false,
                SupportsSpindleOnOff = false,
                SupportsSpindleSpeed = false,
                SupportsFeedHold = false,
                SupportsSoftwareStop = false,
                SupportsWorkOffsets = false,
                SupportsLimitReporting = false,
                SupportsPositionReporting = false,
                SupportsFirmwareUpload = true,
                SupportedAxes = new List<string> { "X", "Y", "Z" },
                WorkspaceLimitX = _profile.XLimitMm,
                WorkspaceLimitY = _profile.YLimitMm,
                WorkspaceLimitZ = _profile.ZLimitMm
            },
            FirmwareWarnings =
            {
                "Firmware capabilities inferred from the legacy serial profile.",
                string.IsNullOrWhiteSpace(startupBanner) ? "No startup identity banner was detected." : $"Startup banner: {startupBanner}"
            }
        };
        ApplyFutureIdentityPayloads(startupBanner, identity);
        return identity;
    }

    private CncFirmwareIdentity CreateGenericFirmwareIdentity(string? startupBanner)
    {
        var hasIdentityPayload = !string.IsNullOrWhiteSpace(startupBanner)
                                 && (startupBanner.Contains("$I", StringComparison.OrdinalIgnoreCase)
                                     || startupBanner.Contains("$VER", StringComparison.OrdinalIgnoreCase)
                                     || startupBanner.Contains("$CAPS", StringComparison.OrdinalIgnoreCase));
        var identity = new CncFirmwareIdentity
        {
            FirmwareName = "Custom CNC Firmware",
            FirmwareVersion = DeviceStatus.FirmwareVersion ?? "Unknown",
            ProtocolName = "Custom",
            ProtocolVersion = new CncProtocolVersion
            {
                ProtocolName = "Custom",
                RawVersion = DeviceStatus.ProtocolVersion ?? "Custom"
            },
            Confidence = hasIdentityPayload ? CncCapabilityConfidence.Verified : CncCapabilityConfidence.Inferred,
            Capabilities = new CncFirmwareCapabilities
            {
                SupportsStatusQuery = true,
                SupportsUnlock = true,
                SupportsMotorEnable = true,
                SupportsMotorDisable = true,
                SupportsHoming = true,
                SupportsJog = true,
                SupportsG0G1 = true,
                SupportsG2G3 = false,
                SupportsSpindleOnOff = false,
                SupportsSpindleSpeed = false,
                SupportsFeedHold = Capabilities.SupportsPause,
                SupportsSoftwareStop = true,
                SupportsWorkOffsets = Capabilities.SupportsWorkCoordinateSystem,
                SupportsLimitReporting = true,
                SupportsPositionReporting = Capabilities.SupportsLivePositionReporting,
                SupportsFirmwareUpload = true,
                SupportedAxes = new List<string> { "X", "Y", "Z" },
                WorkspaceLimitX = _profile.XLimitMm,
                WorkspaceLimitY = _profile.YLimitMm,
                WorkspaceLimitZ = _profile.ZLimitMm
            },
            FirmwareWarnings =
            {
                "Firmware capabilities inferred from custom driver assumptions."
            }
        };
        ApplyFutureIdentityPayloads(startupBanner, identity);
        return identity;
    }

    private string? TryEnhanceFirmwareIdentity(string? startupBanner)
    {
        if (!IsConnected || _serialPort == null || UsesSimpleFirmwareProtocol)
            return startupBanner;

        var handshakeLines = new List<string>();
        foreach (var command in new[] { "$I", "$VER", "$CAPS" })
        {
            try
            {
                _serialPort.WriteLine(command);
                var deadline = DateTime.UtcNow.AddMilliseconds(400);
                while (DateTime.UtcNow < deadline)
                {
                    _serialPort.ReadTimeout = Math.Max(80, (int)(deadline - DateTime.UtcNow).TotalMilliseconds);
                    var rawLine = _serialPort.ReadLine()?.Trim();
                    if (string.IsNullOrWhiteSpace(rawLine))
                        continue;

                    if (rawLine.StartsWith("$I", StringComparison.OrdinalIgnoreCase)
                        || rawLine.StartsWith("$VER", StringComparison.OrdinalIgnoreCase)
                        || rawLine.StartsWith("$CAPS", StringComparison.OrdinalIgnoreCase))
                    {
                        handshakeLines.Add(rawLine);
                    }
                    else if (rawLine.StartsWith("ERR:", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                }
            }
            catch (TimeoutException)
            {
                // Future hook: older firmware simply won't answer.
            }
            catch (Exception)
            {
                break;
            }
        }

        if (handshakeLines.Count == 0)
            return startupBanner;

        MarkVerifiedStatus();
        return string.Join(Environment.NewLine, new[] { startupBanner ?? string.Empty }.Concat(handshakeLines).Where(line => !string.IsNullOrWhiteSpace(line)));
    }

    private static void ApplyFutureIdentityPayloads(string? text, CncFirmwareIdentity identity)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        var lines = text
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var line in lines)
        {
            if (line.StartsWith("$I", StringComparison.OrdinalIgnoreCase))
                ApplyIdentityLine(line, identity);
            else if (line.StartsWith("$VER", StringComparison.OrdinalIgnoreCase))
                ApplyVersionLine(line, identity);
            else if (line.StartsWith("$CAPS", StringComparison.OrdinalIgnoreCase))
                ApplyCapabilitiesLine(line, identity);
        }
    }

    private static void ApplyIdentityLine(string line, CncFirmwareIdentity identity)
    {
        var payload = line[(line.IndexOf(':') + 1)..];
        foreach (var entry in payload.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = entry.Split('=', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
                continue;

            switch (parts[0].ToUpperInvariant())
            {
                case "NAME":
                    identity.FirmwareName = parts[1];
                    break;
                case "MACHINE":
                    identity.MachineId = parts[1];
                    break;
                case "BUILD":
                    identity.BuildDate = parts[1];
                    break;
            }
        }
    }

    private static void ApplyVersionLine(string line, CncFirmwareIdentity identity)
    {
        var payload = line[(line.IndexOf(':') + 1)..].Trim();
        if (string.IsNullOrWhiteSpace(payload))
            return;

        identity.FirmwareVersion = payload;
        identity.ProtocolVersion.RawVersion = payload;
    }

    private static void ApplyCapabilitiesLine(string line, CncFirmwareIdentity identity)
    {
        var payload = line[(line.IndexOf(':') + 1)..];
        foreach (var token in payload.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            switch (token.ToUpperInvariant())
            {
                case "STATUS":
                    identity.Capabilities.SupportsStatusQuery = true;
                    break;
                case "UNLOCK":
                    identity.Capabilities.SupportsUnlock = true;
                    break;
                case "MOTORENABLE":
                    identity.Capabilities.SupportsMotorEnable = true;
                    break;
                case "MOTORDISABLE":
                    identity.Capabilities.SupportsMotorDisable = true;
                    break;
                case "HOME":
                    identity.Capabilities.SupportsHoming = true;
                    break;
                case "JOG":
                    identity.Capabilities.SupportsJog = true;
                    break;
                case "G0G1":
                    identity.Capabilities.SupportsG0G1 = true;
                    break;
                case "G2G3":
                    identity.Capabilities.SupportsG2G3 = true;
                    break;
                case "SPINDLE":
                    identity.Capabilities.SupportsSpindleOnOff = true;
                    break;
                case "SPINDLESPEED":
                    identity.Capabilities.SupportsSpindleSpeed = true;
                    break;
                case "FEEDHOLD":
                    identity.Capabilities.SupportsFeedHold = true;
                    break;
                case "STOP":
                    identity.Capabilities.SupportsSoftwareStop = true;
                    break;
                case "WCO":
                    identity.Capabilities.SupportsWorkOffsets = true;
                    break;
                case "LIMITS":
                    identity.Capabilities.SupportsLimitReporting = true;
                    break;
                case "POSITION":
                    identity.Capabilities.SupportsPositionReporting = true;
                    break;
            }
        }
    }
}
