namespace MabaControlCenter.Models;

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Direction { get; set; } = "";
    public string Message { get; set; } = "";
    public string Status { get; set; } = "";
}
