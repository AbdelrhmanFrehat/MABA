using Maba.Domain.Common;

namespace Maba.Domain.Printing;

public class PrintJob : BaseEntity
{
    public Guid SlicingJobId { get; set; }
    public Guid PrinterId { get; set; }
    public Guid PrintJobStatusId { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public decimal? ActualMaterialGrams { get; set; }
    public int? ActualTimeMin { get; set; }
    public decimal? FinalPrice { get; set; }
    public int ProgressPercent { get; set; } = 0;
    public DateTime? EstimatedCompletionTime { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Navigation properties
    public SlicingJob SlicingJob { get; set; } = null!;
    public Printer Printer { get; set; } = null!;
    public PrintJobStatus PrintJobStatus { get; set; } = null!;
}

