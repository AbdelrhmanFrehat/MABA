using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class UpdateService : IUpdateService
{
    private AppUpdateInfo _info = new()
    {
        CurrentVersion = "0.1.0",
        LatestVersion = "0.2.0",
        IsUpdateAvailable = true,
        StatusMessage = "Update available"
    };

    public AppUpdateInfo GetUpdateInfo() => _info;

    public event EventHandler? UpdateInfoChanged;

    public void CheckForUpdates()
    {
        _info = new AppUpdateInfo
        {
            CurrentVersion = "0.1.0",
            LatestVersion = "0.2.0",
            IsUpdateAvailable = true,
            StatusMessage = "Update available"
        };
        UpdateInfoChanged?.Invoke(this, EventArgs.Empty);
    }
}
