using Maba.Domain.Common;

namespace Maba.Domain.DesignCad;

public class DesignCadServiceRequestAttachment : BaseEntity
{
    public Guid RequestId { get; set; }
    public DesignCadServiceRequest Request { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
