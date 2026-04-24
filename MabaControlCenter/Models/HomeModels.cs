using System.Windows.Input;

namespace MabaControlCenter.Models;

public class HomeActionCard
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICommand? Command { get; set; }
}

public class HomeUpdateItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class HomeTickerItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "Info";
    public string? ImageUri { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? StartsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
    public string TargetPlatform { get; set; } = "Desktop";

    public bool ShouldDisplay(DateTimeOffset now)
    {
        if (!IsActive)
        {
            return false;
        }

        if (!string.Equals(TargetPlatform, "Desktop", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(TargetPlatform, "All", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return (!StartsAt.HasValue || StartsAt.Value <= now)
            && (!EndsAt.HasValue || EndsAt.Value >= now);
    }
}

public class HomeModuleRow
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ActionText { get; set; } = "Coming Soon";
    public bool IsAvailable { get; set; }
    public ICommand? Command { get; set; }
}
