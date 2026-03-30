namespace MabaControlCenter.Services;

public interface IThemeService
{
    string CurrentTheme { get; }
    void ApplyTheme(string themeName);
    event EventHandler? ThemeChanged;
}
