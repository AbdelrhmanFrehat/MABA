using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using MabaControlCenter.Models;
using MabaControlCenter.Services;

namespace MabaControlCenter.ViewModels;

public class LogsViewModel : ViewModelBase
{
    private readonly ILoggingService _loggingService;

    public LogsViewModel(ILoggingService loggingService)
    {
        _loggingService = loggingService;
        ClearLogsCommand = new RelayCommand(_ => _loggingService.ClearLogs());
        ExportLogsCommand = new RelayCommand(_ => ExportLogs());
    }

    public string Title => "Logs";
    public string PlaceholderText => "Central log — all sent and received messages.";

    public ObservableCollection<LogEntry> Logs => _loggingService.Logs;

    public ICommand ClearLogsCommand { get; }
    public ICommand ExportLogsCommand { get; }

    private void ExportLogs()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            DefaultExt = ".txt",
            FileName = $"MabaControlCenter_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
        };
        if (dialog.ShowDialog() != true) return;

        var lines = _loggingService.Logs
            .Select(e => $"{e.Timestamp:yyyy-MM-dd HH:mm:ss}\t{e.Direction}\t{e.Message}\t{e.Status}");
        File.WriteAllLines(dialog.FileName, lines);
    }
}
