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

public class HomeModuleRow
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ActionText { get; set; } = "Coming Soon";
    public bool IsAvailable { get; set; }
    public ICommand? Command { get; set; }
}
