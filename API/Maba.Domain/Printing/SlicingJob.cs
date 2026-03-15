using Maba.Domain.Common;

namespace Maba.Domain.Printing;

public class SlicingJob : BaseEntity
{
    public Guid DesignFileId { get; set; }
    public Guid SlicingProfileId { get; set; }
    public Guid SlicingJobStatusId { get; set; }
    public int? EstimatedTimeMin { get; set; }
    public decimal? EstimatedMaterialGrams { get; set; }
    public decimal? PriceEstimate { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string? ErrorMessage { get; set; }
    public string? OutputFileUrl { get; set; } // G-code file URL
    public DateTime? CompletedAt { get; set; }
    
    // Navigation properties
    public DesignFile DesignFile { get; set; } = null!;
    public SlicingProfile SlicingProfile { get; set; } = null!;
    public SlicingJobStatus SlicingJobStatus { get; set; } = null!;
    public ICollection<PrintJob> PrintJobs { get; set; } = new List<PrintJob>();
}

