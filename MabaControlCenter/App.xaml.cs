using System.Diagnostics;
using System.Windows;
using MabaControlCenter.Localization;
using MabaControlCenter.Services;

namespace MabaControlCenter;

public partial class App : Application
{
    /// <summary>Set to true when the app should restart after exit (e.g. after language change).</summary>
    public static bool RestartRequested { get; set; }

    public App()
    {
        InitializeComponent();
        var localizationService = new LocalizationService();
        var labels = new LocalizedLabels(localizationService);
        Resources["LocalizationService"] = localizationService;
        Resources["Labels"] = labels;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (RestartRequested && Environment.ProcessPath is { } path)
        {
            try
            {
                Process.Start(path);
            }
            catch { /* ignore */ }
        }
        base.OnExit(e);
    }
}
