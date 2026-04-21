namespace MabaControlCenter.Models;

public class CncLoadedJobInfo
{
    public string FileName { get; set; } = "No file loaded";
    public string FilePath { get; set; } = string.Empty;
    public string JobTitle { get; set; } = "Local CNC Program";
    public string SourceReference { get; set; } = "Standalone file";
    public int MotionLineCount { get; set; }
    public string ActiveProfileName { get; set; } = string.Empty;
    public string DriverMode { get; set; } = string.Empty;
    public DateTime LoadedAt { get; set; } = DateTime.Now;
}
