using System.Collections.ObjectModel;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface ILoggingService
{
    ObservableCollection<LogEntry> Logs { get; }
    void AddLog(string direction, string message, string status);
    void ClearLogs();
}
