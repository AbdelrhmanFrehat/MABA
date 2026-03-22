using System.Collections.ObjectModel;
using System.Linq;
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

    public string AppVersion => "0.1.0";

    private void SaveCurrentSettings()
    {
        _settingsService.Save(new AppSettings
        {
            Theme = _selectedTheme,
            Language = _selectedLanguage?.CultureCode ?? _localizationService.CurrentCulture,
            StartWithWindows = _startWithWindows,
            CheckForUpdatesAutomatically = _checkForUpdatesAuto,
            DiagnosticsMode = _diagnosticsMode
        });
    }
}
