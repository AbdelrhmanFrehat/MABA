using Maba.Domain.Common;

namespace Maba.Domain.Printing;

public class PrintJobStatus : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<PrintJob> PrintJobs { get; set; } = new List<PrintJob>();
}

