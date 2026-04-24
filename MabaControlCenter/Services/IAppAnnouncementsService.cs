using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface IAppAnnouncementsService
{
    Task<IReadOnlyList<HomeTickerItem>> GetActiveDesktopAnnouncementsAsync(CancellationToken cancellationToken = default);
}
