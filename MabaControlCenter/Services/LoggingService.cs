using System.Collections.ObjectModel;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class LoggingService : ILoggingService
{
    public ObservableCollection<LogEntry> Logs { get; } = new();

    public void AddLog(string direction, string message, string status)
    {
        var entry = new LogEntry
        {
            Timestamp = DateTime.Now,
            Direction = direction,
            Message = message,
            Status = status
        };
        Logs.Insert(0, entry);
    }

    public void ClearLogs()
    {
        Logs.Clear();
    }
}
