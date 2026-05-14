using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MabaUpdater;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly UpdaterArguments _arguments;
    private readonly UpdateInstallService _service = new();
    private string _statusTitle = "Preparing";
    private string _statusSubtitle = "Starting update helper.";
    private bool _isIndeterminate = true;
    private double _progressValue;

    public MainWindow(UpdaterArguments arguments)
    {
        _arguments = arguments;
        InitializeComponent();
        DataContext = this;
        Loaded += OnLoaded;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string StatusTitle
    {
        get => _statusTitle;
        private set => SetField(ref _statusTitle, value);
    }

    public string StatusSubtitle
    {
        get => _statusSubtitle;
        private set => SetField(ref _statusSubtitle, value);
    }

    public bool IsIndeterminate
    {
        get => _isIndeterminate;
        private set => SetField(ref _isIndeterminate, value);
    }

    public double ProgressValue
    {
        get => _progressValue;
        private set => SetField(ref _progressValue, value);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await _service.RunAsync(_arguments, ReportStatus, CancellationToken.None);
            await Task.Delay(900);
            Close();
        }
        catch (Exception ex)
        {
            StatusTitle = "Update failed";
            StatusSubtitle = ex.Message;
            IsIndeterminate = false;
            ProgressValue = 0;
            MessageBox.Show(
                ex.Message,
                "MABA Updater",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Close();
        }
    }

    private void ReportStatus(string title, string subtitle, bool indeterminate, double? progress)
    {
        Dispatcher.Invoke(() =>
        {
            StatusTitle = title;
            StatusSubtitle = subtitle;
            IsIndeterminate = indeterminate;
            ProgressValue = progress ?? ProgressValue;
        });
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
