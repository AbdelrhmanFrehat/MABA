using System.Collections.ObjectModel;

namespace MabaControlCenter.Models;

public class GcodeParseResult
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int TotalLines { get; set; }
    public ObservableCollection<GcodeMotionCommand> Motions { get; } = new();
    public ObservableCollection<string> Messages { get; } = new();
    public int WarningCount { get; set; }
    public int ErrorCount { get; set; }
}
