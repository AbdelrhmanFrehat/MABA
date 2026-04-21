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
        var machineCatalogService = new MachineCatalogService(settingsService);
        var runtimeProfileService = new RuntimeProfileService();
        var effectiveCapabilitiesResolver = new EffectiveCapabilitiesResolver();
        var activeMachineContextService = new ActiveMachineContextService(effectiveCapabilitiesResolver);
        var cncProfileService = new CncProfileService();
        var cncDriverFactory = new CncDriverFactory(loggingService);
        var cncControllerService = new CncControllerService(loggingService, cncProfileService, cncDriverFactory);
        var gcodeParserService = new GcodeParserService();
        var cncExecutionQueueService = new CncExecutionQueueService();
        var cncJobSessionService = new CncJobSessionService();
        var cncPreviewPlaybackService = new CncPreviewPlaybackService();
        var cncFramePathService = new CncFramePathService();
        var cncJobPlacementService = new CncJobPlacementService();
        var localizationService = (ILocalizationService)Application.Current.Resources["LocalizationService"];

        // Load saved settings and apply theme + language on startup
        var settings = settingsService.Load();
        themeService.ApplyTheme(settings.Theme);
        localizationService.SetCulture(settings.Language);

        var nav = new NavigationService(deviceService, loggingService, moduleService, updateService, newsService, themeService, localizationService, settingsService, jobsService, activeProductionJobService, cncProfileService, cncControllerService, gcodeParserService, cncExecutionQueueService, cncJobSessionService, cncPreviewPlaybackService, cncFramePathService, cncJobPlacementService, machineCatalogService, runtimeProfileService, activeMachineContextService);
        DataContext = new MainViewModel(nav);
        Loaded += (_, _) =>
        {
            FlowDirection = localizationService.FlowDirection;
            localizationService.CultureChanged += (_, _) => FlowDirection = localizationService.FlowDirection;
        };
    }
}
