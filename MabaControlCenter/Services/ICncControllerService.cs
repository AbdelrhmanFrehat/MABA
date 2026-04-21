using System.Collections.ObjectModel;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface ICncControllerService
{
    ObservableCollection<LogEntry> SerialLogs { get; }
    bool IsConnected { get; }
    string? ConnectedPort { get; }
    bool MotorsEnabled { get; }
    bool IsHomed { get; }
    bool HasValidMachineReference { get; }
    CncMachineState MachineState { get; }
    CncDeviceStatusSnapshot DeviceStatus { get; }
    CncDriverCapabilities DriverCapabilities { get; }
    CncMachineBounds Bounds { get; }
    decimal MachineX { get; }
    decimal MachineY { get; }
    decimal MachineZ { get; }
    decimal WorkX { get; }
    decimal WorkY { get; }
    decimal WorkZ { get; }
    decimal WorkOffsetX { get; }
    decimal WorkOffsetY { get; }
    decimal WorkOffsetZ { get; }
    string? LastFaultReason { get; }
    string? LastWarning { get; }
    CncMachineConfig Config { get; }
    event EventHandler? StateChanged;
    event EventHandler? ConnectionLost;

    IEnumerable<string> GetAvailablePorts();
    void Connect(string portName);
    void Disconnect();
    string EnableMotors();
    string DisableMotors();
    string AutoHome();
    string GoToCenter();
    string SetWorkZero();
    string ResetState();
    string Stop();
    string RefreshStatus();
    string Jog(string axis, decimal deltaMm);
    void UpdateConfig(CncMachineConfig config);
    string? ValidateMachinePosition(decimal machineX, decimal machineY, decimal machineZ);
    string? ValidateWorkPosition(decimal workX, decimal workY, decimal workZ);
    void ClearWarning();
    string ResetAlarm();
    void SetMachineState(CncMachineState state, string? reason = null);
}
