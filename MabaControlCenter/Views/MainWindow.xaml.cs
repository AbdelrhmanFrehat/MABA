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
        var settingsService = new SettingsService();
        var updateService = new UpdateService(settingsService);
        var newsService = new NewsService();
        var themeService = new ThemeService();
        var authSessionService = new AuthSessionService(settingsService);
        var appAnnouncementsService = new AppAnnouncementsService(settingsService, authSessionService);
        var jobsService = new JobsService(settingsService, authSessionService);
        var activeProductionJobService = new ActiveProductionJobService();
        var machineCatalogService = new MachineCatalogService(settingsService);
        var runtimeProfileService = new RuntimeProfileService();
        var effectiveCapabilitiesResolver = new EffectiveCapabilitiesResolver();
        var activeMachineContextService = new ActiveMachineContextService(effectiveCapabilitiesResolver);
        var cncProfileService = new CncProfileService();
        var cncDriverFactory = new CncDriverFactory(loggingService);
        var cncCoordinateTransformService = new CncCoordinateTransformService();
        var cncControllerService = new CncControllerService(loggingService, cncProfileService, cncDriverFactory, cncCoordinateTransformService);
        var gcodeParserService = new GcodeParserService();
        var cncExecutionPlannerService = new CncExecutionPlannerService(activeMachineContextService, cncCoordinateTransformService);
        var cncExecutionQueueService = new CncExecutionQueueService(cncExecutionPlannerService);
        var cncJobSessionService = new CncJobSessionService();
        var cncPreviewPlaybackService = new CncPreviewPlaybackService();
        var cncFramePathService = new CncFramePathService();
        var cncJobPlacementService = new CncJobPlacementService(cncCoordinateTransformService);
        var cncControllerStateMachine = new CncControllerStateMachine();
        var cncRecoveryPlannerService = new CncRecoveryPlannerService();
        var cncRuntimeActionPolicy = new CncRuntimeActionPolicyService(cncControllerStateMachine);
        var cncRuntimeCoordinator = new CncRuntimeCoordinator(cncControllerService, cncExecutionQueueService, cncJobSessionService, activeMachineContextService, cncControllerStateMachine, cncRecoveryPlannerService, cncRuntimeActionPolicy);
        var cncExecutionPreflightService = new CncExecutionPreflightService(cncCoordinateTransformService);
        var cncManagerService = new CncManagerService(cncControllerService, cncExecutionQueueService, cncJobSessionService, cncRuntimeCoordinator, cncExecutionPreflightService, cncCoordinateTransformService);
        var imagePreprocessorService = new ImagePreprocessorService();
        var imageVectorTraceService = new OutlineVectorTraceService();
        var imageVectorImportService = new SvgVectorImportService();
        var imageToolpathGeneratorService = new ToolpathFromVectorService();
        var imageToolpathService = new ImageToolpathService(imagePreprocessorService, imageVectorTraceService, imageToolpathGeneratorService, imageVectorImportService);
        var localizationService = (ILocalizationService)Application.Current.Resources["LocalizationService"];

        // Load saved settings and apply theme + language on startup
        var settings = settingsService.Load();
        themeService.ApplyTheme(settings.Theme);
        localizationService.SetCulture(settings.Language);

        var nav = new NavigationService(deviceService, loggingService, appAnnouncementsService, moduleService, updateService, newsService, themeService, localizationService, settingsService, jobsService, activeProductionJobService, cncProfileService, cncControllerService, gcodeParserService, cncExecutionQueueService, cncJobSessionService, cncPreviewPlaybackService, cncFramePathService, cncJobPlacementService, machineCatalogService, runtimeProfileService, activeMachineContextService, cncRuntimeCoordinator, cncCoordinateTransformService, cncManagerService, imageToolpathService, authSessionService);
        DataContext = new MainViewModel(nav, authSessionService);
        if (settings.CheckForUpdatesAutomatically)
            _ = updateService.CheckForUpdatesAsync(userInitiated: false);
        Loaded += (_, _) =>
        {
            FlowDirection = localizationService.FlowDirection;
            localizationService.CultureChanged += (_, _) => FlowDirection = localizationService.FlowDirection;
        };
    }
}
