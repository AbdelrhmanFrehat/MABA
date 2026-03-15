using Maba.Domain.Common;

namespace Maba.Domain.Printing;

public class SlicingJobStatus : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<SlicingJob> SlicingJobs { get; set; } = new List<SlicingJob>();
}

