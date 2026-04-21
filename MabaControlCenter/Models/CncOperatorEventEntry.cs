namespace MabaControlCenter.Models;

public class CncOperatorEventEntry
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Severity { get; set; } = "Info";
    public string Message { get; set; } = string.Empty;
}
