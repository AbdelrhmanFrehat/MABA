using Maba.Domain.Common;
using Maba.Domain.Media;

namespace Maba.Domain.Printing;

public class DesignFile : BaseEntity
{
    public Guid DesignId { get; set; }
    public Guid MediaAssetId { get; set; }
    public string Format { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public long FileSizeBytes { get; set; }
    public bool IsPrimary { get; set; } = false;
    
    // Navigation properties
    public Design Design { get; set; } = null!;
    public MediaAsset MediaAsset { get; set; } = null!;
    public ICollection<SlicingJob> SlicingJobs { get; set; } = new List<SlicingJob>();
}

