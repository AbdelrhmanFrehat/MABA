namespace MabaControlCenter.Models;

public static class ControlCenterJobDisplay
{
    public static string FormatStatus(string? status) => status switch
    {
        "InProgress" => "In Progress",
        null or "" => "Unknown",
        _ => status
    };

    public static string FormatMachineType(string? machineType) => machineType switch
    {
        "PRINTER_3D" => "3D Printer",
        null or "" => "Unknown",
        _ => machineType
    };

    public static string FormatSourceType(string? sourceType) => sourceType switch
    {
        "PRINT_REQUEST" => "3D Print Request",
        "CNC_REQUEST" => "CNC Request",
        "LASER_REQUEST" => "Laser Request",
        "ORDER" => "Order",
        null or "" => "Unknown",
        _ => sourceType
    };
}

public class JobFilterOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

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
    public string StatusDisplay => ControlCenterJobDisplay.FormatStatus(Status);
    public string MachineTypeDisplay => ControlCenterJobDisplay.FormatMachineType(MachineType);
    public string SourceTypeDisplay => ControlCenterJobDisplay.FormatSourceType(SourceType);
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
