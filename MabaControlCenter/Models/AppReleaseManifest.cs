namespace MabaControlCenter.Models;

public class AppReleaseManifest
{
    public string Version { get; set; } = string.Empty;
    public string PackageUri { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTimeOffset? PublishedAt { get; set; }
}
