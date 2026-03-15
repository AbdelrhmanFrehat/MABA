using Maba.Domain.Common;

namespace Maba.Domain.Design;

public class DesignServiceRequestAttachment : BaseEntity
{
    public Guid RequestId { get; set; }
    public DesignServiceRequest Request { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;
    /// <summary>Storage path (relative) for retrieval.</summary>
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
