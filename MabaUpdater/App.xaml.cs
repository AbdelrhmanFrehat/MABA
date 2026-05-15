using System.Windows;

namespace MabaUpdater;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        UpdaterLog.Write($"Updater starting. Args: {string.Join(" ", e.Args)}");

        UpdaterArguments args;
        try
        {
            args = UpdaterArguments.Parse(e.Args);
            UpdaterLog.Write($"Arguments parsed. Source={args.SourceDirectory}; Target={args.TargetDirectory}; Relaunch={args.RelaunchPath}; Version={args.Version}");
        }
        catch (Exception ex)
        {
            UpdaterLog.Write($"Argument parsing failed: {ex}");
            MessageBox.Show(
                ex.Message,
                "MABA Updater",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
            return;
        }

        try
        {
            UpdaterLog.Write("Creating updater window.");
            var window = new MainWindow(args);
            MainWindow = window;
            UpdaterLog.Write("Showing updater window.");
            window.Show();
        }
        catch (Exception ex)
        {
            UpdaterLog.Write($"Updater window startup failed: {ex}");
            MessageBox.Show(
                ex.Message,
                "MABA Updater",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
        }
    }
}
