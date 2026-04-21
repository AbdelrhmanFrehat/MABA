namespace MabaControlCenter.Models;

public enum CncDeviceState
{
    Unknown,
    Ready,
    Idle,
    Running,
    Homing,
    Paused,
    Stopped,
    Alarm,
    Error,
    LimitHit,
    Disconnected
}
