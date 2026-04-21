using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using MabaControlCenter.Localization;
using MabaControlCenter.Services;
using MabaControlCenter.Views;

namespace MabaControlCenter;

public partial class App : Application
{
    /// <summary>Set to true when the app should restart after exit (e.g. after language change).</summary>
    public static bool RestartRequested { get; set; }

    private static string StartupErrorLogPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MabaControlCenter", "startup-error.log");

    public App()
    {
        InitializeComponent();
        var localizationService = new LocalizationService();
        var labels = new LocalizedLabels(localizationService);
        Resources["LocalizationService"] = localizationService;
        Resources["Labels"] = labels;
        DispatcherUnhandledException += OnDispatcherUnhandledException;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            base.OnStartup(e);
            var window = new MainWindow();
            MainWindow = window;
            window.Show();
        }
        catch (Exception ex)
        {
            WriteStartupError(ex);
            MessageBox.Show(
                ex.ToString(),
                "Maba Control Center Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        WriteStartupError(e.Exception);
        MessageBox.Show(
            e.Exception.ToString(),
            "Maba Control Center Unhandled Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
        Shutdown(-1);
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

    private static void WriteStartupError(Exception ex)
    {
        try
        {
            var directory = Path.GetDirectoryName(StartupErrorLogPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(
                StartupErrorLogPath,
                $"[{DateTime.Now:O}]{Environment.NewLine}{ex}");
        }
        catch
        {
            // If logging fails, there's nothing else we can safely do here.
        }
    }
}
