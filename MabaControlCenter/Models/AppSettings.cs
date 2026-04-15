namespace MabaControlCenter.Models;

/// <summary>User preferences persisted to local storage.</summary>
public class AppSettings
{
    public string Theme { get; set; } = "MABA";
    public string Language { get; set; } = "en";
    public bool StartWithWindows { get; set; }
    public bool CheckForUpdatesAutomatically { get; set; } = true;
    public bool DiagnosticsMode { get; set; }
    public string ApiBaseUrl { get; set; } = "https://api.mabasol.com";
}
