namespace MabaControlCenter.Models;

public enum CncMachineState
{
    Disconnected,
    Idle,
    Homing,
    Running,
    Paused,
    Stopped,
    Completed,
    Warning,
    Alarm,
    Error
}
