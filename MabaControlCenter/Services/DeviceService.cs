using System.IO.Ports;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class DeviceService : IDeviceService
{
    private readonly ILoggingService _logging;
    private SerialPort? _serialPort;
    private const int BaudRate = 9600;

    public DeviceService(ILoggingService logging)
    {
        _logging = logging;
    }

    /// <summary>
    /// When true, no real serial port is used; Connect/Disconnect only change status, SendCommand returns fake responses.
    /// </summary>
    public bool IsSimulationMode { get; set; } = true;

    private bool _simulationConnected;
    private DeviceProfile? _connectedProfile;

    public bool IsConnected => IsSimulationMode ? _simulationConnected : (_serialPort?.IsOpen ?? false);
    public string? ConnectedProductCode => _connectedProfile?.ProductCode;
    public string? ConnectedDeviceName => _connectedProfile?.ProductName;
    public string? ConnectedRecommendedModule => _connectedProfile?.RecommendedModule;

    public event EventHandler? ConnectionStateChanged;

    public IEnumerable<string> GetAvailablePorts()
    {
        return SerialPort.GetPortNames().OrderBy(name => name).ToArray();
    }

    public void Connect(string portName)
    {
        if (IsSimulationMode)
        {
            _simulationConnected = true;
            _connectedProfile = CreateSimulatedProfile();
            ConnectionStateChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (string.IsNullOrWhiteSpace(portName))
            return;

        if (_serialPort?.IsOpen == true)
            return;

        _serialPort = new SerialPort(portName, BaudRate);
        _serialPort.Open();
        ConnectionStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public DeviceProfile? PerformHandshake()
    {
        if (IsSimulationMode)
        {
            if (!_simulationConnected) return null;
            return _connectedProfile ?? CreateSimulatedProfile();
        }
        if (_serialPort == null || !_serialPort.IsOpen) return null;
        return null;
    }

    public void AssumeDexterForMacroPadModule()
    {
        if (!IsConnected) return;
        if (IsSimulationMode) return;
        _connectedProfile = new DeviceProfile
        {
            ProductName = "MABA Dexter",
            ProductCode = "DEXTER-VP1",
            FirmwareVersion = "",
            SerialNumber = "",
            ConnectionType = "USB",
            IsSupported = true,
            RecommendedModule = "Dexter MacroPad"
        };
        ConnectionStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private static DeviceProfile CreateSimulatedProfile()
    {
        return new DeviceProfile
        {
            ProductName = "MABA Dexter VP1",
            ProductCode = "DEXTER-VP1",
            FirmwareVersion = "1.0.0",
            SerialNumber = "MABA-0001",
            ConnectionType = "USB",
            IsSupported = true,
            RecommendedModule = "Dexter MacroPad"
        };
    }

    public void Disconnect()
    {
        if (IsSimulationMode)
        {
            _simulationConnected = false;
            _connectedProfile = null;
            ConnectionStateChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        _connectedProfile = null;
        if (_serialPort == null)
            return;
        if (_serialPort.IsOpen)
        {
            _serialPort.Close();
            _serialPort.Dispose();
        }
        _serialPort = null;
        ConnectionStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public string SendCommand(string command)
    {
        _logging.AddLog("Sent", command ?? "", "Info");

        string response;
        if (IsSimulationMode)
        {
            if (!_simulationConnected)
                response = "(not connected)";
            else
                response = GetSimulationResponse(command?.Trim()?.ToUpperInvariant() ?? "");
        }
        else if (_serialPort == null || !_serialPort.IsOpen)
        {
            response = "(not connected)";
        }
        else
        {
            _serialPort.WriteLine(command);
            var previousTimeout = _serialPort.ReadTimeout;
            _serialPort.ReadTimeout = 2000;
            try
            {
                response = _serialPort.ReadLine() ?? "(no response)";
            }
            catch (TimeoutException)
            {
                response = "(no response)";
            }
            finally
            {
                _serialPort.ReadTimeout = previousTimeout;
            }
        }

        var responseStatus = string.Equals(response, "UNKNOWN COMMAND", StringComparison.OrdinalIgnoreCase) ? "Error" : "Info";
        _logging.AddLog("Received", response, responseStatus);

        return response;
    }

    private static string GetSimulationResponse(string command)
    {
        return command switch
        {
            "PING" => "OK",
            "STATUS" => "READY",
            "START" => "RUNNING",
            "STOP" => "STOPPED",
            "MACROPAD_APPLY" => "MACROPAD_OK",
            "MACROPAD_LAYOUT_CALIB" => "LAYOUT_OK",
            _ => "UNKNOWN COMMAND"
        };
    }
}
