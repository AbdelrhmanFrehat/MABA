using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface ISettingsService
{
    /// <summary>Load settings from disk, or return defaults if file is missing.</summary>
    AppSettings Load();

    /// <summary>Save settings to disk.</summary>
    void Save(AppSettings settings);
}
