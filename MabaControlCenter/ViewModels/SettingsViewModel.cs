using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using MabaControlCenter.Models;
using MabaControlCenter.Services;

namespace MabaControlCenter.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly IThemeService _themeService;
    private readonly ILocalizationService _localizationService;
    private readonly ISettingsService _settingsService;
    private string _selectedTheme;
    private LanguageOption? _selectedLanguage;
    private bool _startWithWindows;
    private bool _checkForUpdatesAuto = true;
    private bool _diagnosticsMode;
    private string _updateManifestUri = string.Empty;

    public SettingsViewModel(IThemeService themeService, ILocalizationService localizationService, ISettingsService settingsService)
    {
        _themeService = themeService;
        _localizationService = localizationService;
        _settingsService = settingsService;

        _selectedTheme = _themeService.CurrentTheme;
        _themeService.ThemeChanged += (_, _) =>
        {
            _selectedTheme = _themeService.CurrentTheme;
            OnPropertyChanged(nameof(SelectedTheme));
        };

        ThemeOptions = new ObservableCollection<string> { "MABA", "Dark", "Light" };
        LanguageOptions = new ObservableCollection<LanguageOption>
        {
            new LanguageOption("en", "Language_English", _localizationService),
            new LanguageOption("ar", "Language_Arabic", _localizationService)
        };
        _selectedLanguage = LanguageOptions.FirstOrDefault(o => o.CultureCode == _localizationService.CurrentCulture) ?? LanguageOptions[0];
        _localizationService.CultureChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(PlaceholderText));
        };

        // Load persisted preferences (theme/language already applied at startup)
        var saved = _settingsService.Load();
        _startWithWindows = saved.StartWithWindows;
        _checkForUpdatesAuto = saved.CheckForUpdatesAutomatically;
        _diagnosticsMode = saved.DiagnosticsMode;
        _updateManifestUri = saved.UpdateManifestUri ?? string.Empty;
    }

    public string Title => _localizationService.GetString("Page_Settings_Title");
    public string PlaceholderText => _localizationService.GetString("Page_Settings_Placeholder");

    public ObservableCollection<string> ThemeOptions { get; }
    public ObservableCollection<LanguageOption> LanguageOptions { get; }

    public LanguageOption? SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (_selectedLanguage == value || value == null) return;
            _selectedLanguage = value;
            OnPropertyChanged();
            SaveCurrentSettings();
            _localizationService.SetCulture(value.CultureCode);
            if (Application.Current is App app)
            {
                App.RestartRequested = true;
                app.Shutdown();
            }
        }
    }

    public string SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (_selectedTheme == value) return;
            _selectedTheme = value;
            OnPropertyChanged();
            _themeService.ApplyTheme(value);
            SaveCurrentSettings();
        }
    }

    public bool StartWithWindows
    {
        get => _startWithWindows;
        set { if (_startWithWindows == value) return; _startWithWindows = value; OnPropertyChanged(); SaveCurrentSettings(); }
    }

    public bool CheckForUpdatesAuto
    {
        get => _checkForUpdatesAuto;
        set { if (_checkForUpdatesAuto == value) return; _checkForUpdatesAuto = value; OnPropertyChanged(); SaveCurrentSettings(); }
    }

    public bool DiagnosticsMode
    {
        get => _diagnosticsMode;
        set { if (_diagnosticsMode == value) return; _diagnosticsMode = value; OnPropertyChanged(); SaveCurrentSettings(); }
    }

    public string UpdateManifestUri
    {
        get => _updateManifestUri;
        set
        {
            if (_updateManifestUri == value) return;
            _updateManifestUri = value;
            OnPropertyChanged();
            SaveCurrentSettings();
        }
    }

    public string AppVersion
    {
        get
        {
            var informational = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;
            if (!string.IsNullOrWhiteSpace(informational))
                return informational.Split('+')[0];

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            if (version == null)
                return "0.0.0";

            return version.Build > 0
                ? $"{version.Major}.{version.Minor}.{version.Build}"
                : $"{version.Major}.{version.Minor}.0";
        }
    }

    private void SaveCurrentSettings()
    {
        var current = _settingsService.Load();
        current.Theme = _selectedTheme;
        current.Language = _selectedLanguage?.CultureCode ?? _localizationService.CurrentCulture;
        current.StartWithWindows = _startWithWindows;
        current.CheckForUpdatesAutomatically = _checkForUpdatesAuto;
        current.DiagnosticsMode = _diagnosticsMode;
        current.UpdateManifestUri = _updateManifestUri?.Trim() ?? string.Empty;
        _settingsService.Save(current);
    }
}
