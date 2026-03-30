namespace MabaControlCenter.Models;

public class AppUpdateInfo
{
    public string CurrentVersion { get; set; } = "";
    public string LatestVersion { get; set; } = "";
    public bool IsUpdateAvailable { get; set; }
    public string StatusMessage { get; set; } = "";
}
