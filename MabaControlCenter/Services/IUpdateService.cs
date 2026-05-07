using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface IUpdateService
{
    AppUpdateInfo GetUpdateInfo();
    Task CheckForUpdatesAsync(bool userInitiated = true, CancellationToken cancellationToken = default);
    Task<bool> InstallUpdateAsync(CancellationToken cancellationToken = default);
    event EventHandler? UpdateInfoChanged;
}
