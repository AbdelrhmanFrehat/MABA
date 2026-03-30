using System.IO;
using System.Windows;

namespace MabaControlCenter.Services;

public class ThemeService : IThemeService
{
    private const int ThemeDictionaryIndex = 0;
    private static string ThemeFilePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MabaControlCenter", "theme.txt");

    private string _currentTheme = "MABA";

    public string CurrentTheme
    {
        get => _currentTheme;
        private set
        {
            if (_currentTheme == value) return;
            _currentTheme = value;
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? ThemeChanged;

    public void ApplyTheme(string themeName)
    {
        if (string.IsNullOrWhiteSpace(themeName)) return;

        var app = Application.Current;
        if (app?.Resources?.MergedDictionaries == null || app.Resources.MergedDictionaries.Count <= ThemeDictionaryIndex) return;

        var normalized = themeName.Trim();
        var uri = normalized.Equals("Light", StringComparison.OrdinalIgnoreCase)
            ? new Uri("pack://application:,,,/MabaControlCenter;component/Themes/LightTheme.xaml", UriKind.Absolute)
            : normalized.Equals("MABA", StringComparison.OrdinalIgnoreCase)
                ? new Uri("pack://application:,,,/MabaControlCenter;component/Themes/MabaTheme.xaml", UriKind.Absolute)
                : new Uri("pack://application:,,,/MabaControlCenter;component/Themes/DarkTheme.xaml", UriKind.Absolute);

        app.Resources.MergedDictionaries[ThemeDictionaryIndex] = new ResourceDictionary { Source = uri };

        if (normalized.Equals("Light", StringComparison.OrdinalIgnoreCase))
            CurrentTheme = "Light";
        else if (normalized.Equals("MABA", StringComparison.OrdinalIgnoreCase))
            CurrentTheme = "MABA";
        else
            CurrentTheme = "Dark";

        SaveTheme(CurrentTheme);
    }

    /// <summary>Call once at startup to apply saved theme (or MABA default).</summary>
    public void ApplySavedOrDefaultTheme()
    {
        var saved = LoadSavedTheme();
        ApplyTheme(string.IsNullOrWhiteSpace(saved) ? "MABA" : saved);
    }

    private static string? LoadSavedTheme()
    {
        try
        {
            if (File.Exists(ThemeFilePath))
                return File.ReadAllText(ThemeFilePath).Trim();
        }
        catch { /* ignore */ }
        return null;
    }

    private static void SaveTheme(string themeName)
    {
        try
        {
            var dir = Path.GetDirectoryName(ThemeFilePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(ThemeFilePath, themeName);
        }
        catch { /* ignore */ }
    }
}
