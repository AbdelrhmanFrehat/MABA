namespace MabaControlCenter.Models;

public enum CncJobLifecycleState
{
    NoJob,
    Loaded,
    Ready,
    Running,
    Paused,
    Stopped,
    Completed,
    Failed,
    Interrupted
}
