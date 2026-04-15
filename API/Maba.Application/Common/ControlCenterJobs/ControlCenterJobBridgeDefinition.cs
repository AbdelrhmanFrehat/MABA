using Maba.Domain.ControlCenter;

namespace Maba.Application.Common.ControlCenterJobs;

public sealed class ControlCenterJobBridgeDefinition
{
    public string SourceType { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public string? SourceReference { get; set; }
    public string? JobReference { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CustomerName { get; set; }
    public string MachineType { get; set; } = string.Empty;
    public Guid? ModuleId { get; set; }
    public CcJobStatus Status { get; set; } = CcJobStatus.Pending;
    public string? Priority { get; set; }
    public Guid? AssignedDeviceId { get; set; }
    public IReadOnlyCollection<ControlCenterJobFileReference> Attachments { get; set; } = Array.Empty<ControlCenterJobFileReference>();
    public string? PayloadJson { get; set; }
    public string? ParametersJson { get; set; }
    public bool CreateIfMissing { get; set; } = true;
}

public sealed class ControlCenterJobFileReference
{
    public string FileName { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string? FileUrl { get; set; }
    public string? Kind { get; set; }
}
