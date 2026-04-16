using System.Windows;
using MabaControlCenter.Services;
using MabaControlCenter.ViewModels;

namespace MabaControlCenter.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var loggingService = new LoggingService();
        var deviceService = new DeviceService(loggingService);
        var moduleService = new ModuleService();
        var updateService = new UpdateService();
        var newsService = new NewsService();
        var themeService = new ThemeService();
        var settingsService = new SettingsService();
        var jobsService = new JobsService(settingsService);
        var activeProductionJobService = new ActiveProductionJobService();
        var localizationService = (ILocalizationService)Application.Current.Resources["LocalizationService"];

        // Load saved settings and apply theme + language on startup
        var settings = settingsService.Load();
        themeService.ApplyTheme(settings.Theme);
        localizationService.SetCulture(settings.Language);

        var nav = new NavigationService(deviceService, loggingService, moduleService, updateService, newsService, themeService, localizationService, settingsService, jobsService, activeProductionJobService);
        DataContext = new MainViewModel(nav);
        Loaded += (_, _) =>
        {
            FlowDirection = localizationService.FlowDirection;
            localizationService.CultureChanged += (_, _) => FlowDirection = localizationService.FlowDirection;
        };
    }
}
