using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface IUpdateService
{
    AppUpdateInfo GetUpdateInfo();
    void CheckForUpdates();
    event EventHandler? UpdateInfoChanged;
}
