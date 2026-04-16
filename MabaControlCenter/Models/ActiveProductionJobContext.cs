namespace MabaControlCenter.Models;

public class ActiveProductionJobContext
{
    public Guid JobId { get; set; }
    public string JobReference { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string? SourceReference { get; set; }
    public string? MachineType { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CustomerName { get; set; }
    public string? PayloadJson { get; set; }
    public IReadOnlyList<ControlCenterJobAttachment> Attachments { get; set; } = Array.Empty<ControlCenterJobAttachment>();

    public string StatusDisplay => ControlCenterJobDisplay.FormatStatus(Status);
    public string MachineTypeDisplay => ControlCenterJobDisplay.FormatMachineType(MachineType);
    public string SourceTypeDisplay => ControlCenterJobDisplay.FormatSourceType(SourceType);
}
