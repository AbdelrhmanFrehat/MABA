using System.Collections.ObjectModel;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface ICncDriver
{
    ObservableCollection<LogEntry> SerialLogs { get; }
    CncDriverType DriverType { get; }
    CncDriverCapabilities Capabilities { get; }
    CncDeviceStatusSnapshot DeviceStatus { get; }
    bool IsConnected { get; }
    string? ConnectedPort { get; }
    bool MotorsEnabled { get; }
    event EventHandler? StateChanged;
    event EventHandler? ConnectionLost;

    IEnumerable<string> GetAvailablePorts();
    void ApplyProfile(CncMachineProfile profile);
    void Connect(string portName);
    void Disconnect();
    string EnableMotors();
    string DisableMotors();
    string AutoHome();
    string ResetAlarm();
    string Stop();
    string RefreshStatus();
    string Jog(string axis, decimal deltaMm);
}
