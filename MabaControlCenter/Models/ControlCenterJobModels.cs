namespace MabaControlCenter.Models;

public class ControlCenterJobListItem
{
    public Guid Id { get; set; }
    public string JobReference { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public Guid? SourceId { get; set; }
    public string? SourceReference { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CustomerName { get; set; }
    public string? MachineType { get; set; }
    public Guid? ModuleId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Priority { get; set; }
    public Guid? AssignedDeviceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ControlCenterJobDetail : ControlCenterJobListItem
{
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ResultSummary { get; set; }
    public List<ControlCenterJobAttachment> Attachments { get; set; } = new();
    public string? PayloadJson { get; set; }
}

public class ControlCenterJobAttachment
{
    public string FileName { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string? FileUrl { get; set; }
    public string? Kind { get; set; }
}

public class JobPayloadEntry
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
