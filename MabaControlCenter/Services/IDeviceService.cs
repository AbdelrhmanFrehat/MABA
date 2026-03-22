using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface IDeviceService
{
    bool IsConnected { get; }
    string? ConnectedProductCode { get; }
    string? ConnectedDeviceName { get; }
    string? ConnectedRecommendedModule { get; }
    bool IsSimulationMode { get; }
    event EventHandler? ConnectionStateChanged;
    IEnumerable<string> GetAvailablePorts();
    void Connect(string portName);
    void Disconnect();
    string SendCommand(string command);
    /// <summary>
    /// Performs handshake and returns device profile. In simulation mode returns a fake profile when connected.
    /// </summary>
    DeviceProfile? PerformHandshake();

    /// <summary>After connecting from the Dexter module, treat the session as Dexter when the main link has no product ID (real COM).</summary>
    void AssumeDexterForMacroPadModule();
}
