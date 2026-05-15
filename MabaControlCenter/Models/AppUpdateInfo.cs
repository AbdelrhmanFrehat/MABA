namespace MabaControlCenter.Models;

public class AppUpdateInfo
{
    public string CurrentVersion { get; set; } = "";
    public string LatestVersion { get; set; } = "";
    public string FeedVersion { get; set; } = "";
    public bool IsUpdateAvailable { get; set; }
    public string StatusMessage { get; set; } = "";
    public string ReleaseNotes { get; set; } = "";
    public string UpdateSource { get; set; } = "";
    public bool IsBusy { get; set; }
    public bool CanInstall { get; set; }
}
