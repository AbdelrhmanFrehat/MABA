using System.Collections.ObjectModel;
using System.Windows.Input;
using MabaControlCenter.Models;
using MabaControlCenter.Services;

namespace MabaControlCenter.ViewModels;

public class CommandsViewModel : ViewModelBase
{
    private readonly IDeviceService _deviceService;
    private string _commandText = "";

    public CommandsViewModel(IDeviceService deviceService, ILoggingService loggingService)
    {
        _deviceService = deviceService;
        _loggingService = loggingService;
        _deviceService.ConnectionStateChanged += (_, _) => CommandManager.InvalidateRequerySuggested();

        SendCommandCommand = new RelayCommand(_ => Send(), _ => CanSend());
    }

    private readonly ILoggingService _loggingService;

    public string Title => "Commands";
    public string PlaceholderText => "Send text commands (e.g. PING, STATUS, START, STOP).";

    public string CommandText
    {
        get => _commandText;
        set
        {
            if (_commandText == value) return;
            _commandText = value ?? "";
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public ObservableCollection<LogEntry> Logs => _loggingService.Logs;

    public ICommand SendCommandCommand { get; }

    private bool CanSend() => _deviceService.IsConnected && !string.IsNullOrWhiteSpace(CommandText?.Trim());

    private void Send()
    {
        var text = CommandText?.Trim();
        if (string.IsNullOrEmpty(text)) return;

        _deviceService.SendCommand(text);
        CommandText = "";
    }
}
