using System.Collections.ObjectModel;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class NewsService : INewsService
{
    public ObservableCollection<NewsItem> NewsItems { get; } = new()
    {
        new NewsItem
        {
            Title = "MABA Control Center launched",
            Description = "Welcome to MABA Control Center. Connect your devices and manage modules from one place.",
            Date = new DateTime(2025, 3, 1)
        },
        new NewsItem
        {
            Title = "Dexter MacroPad module now supported",
            Description = "Configure MacroPad keys (a–p) for MABA Dexter VP1 from Control Center.",
            Date = new DateTime(2025, 3, 10)
        },
        new NewsItem
        {
            Title = "SCARA Control module coming soon",
            Description = "Control panel for MABA SCARA robots will be available in a future update.",
            Date = new DateTime(2025, 3, 15)
        }
    };
}
