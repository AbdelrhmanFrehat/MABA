using System.IO;
using System.Text.Json;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class SettingsService : ISettingsService
{
    private static string SettingsFilePath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MabaControlCenter",
            "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                var json = File.ReadAllText(SettingsFilePath);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                if (loaded != null)
                    return loaded;
            }
        }
        catch { /* ignore */ }

        return new AppSettings();
    }

    public void Save(AppSettings settings)
    {
        if (settings == null) return;
        try
        {
            var dir = Path.GetDirectoryName(SettingsFilePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(SettingsFilePath, json);
        }
        catch { /* ignore */ }
    }
}
