using System.Windows;

namespace MabaUpdater;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        UpdaterArguments args;
        try
        {
            args = UpdaterArguments.Parse(e.Args);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "MABA Updater",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
            return;
        }

        var window = new MainWindow(args);
        MainWindow = window;
        window.Show();
    }
}
