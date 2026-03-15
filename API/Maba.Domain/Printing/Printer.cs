using Maba.Domain.Common;

namespace Maba.Domain.Printing;

public class Printer : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Vendor { get; set; }
    public decimal BuildVolumeX { get; set; }
    public decimal BuildVolumeY { get; set; }
    public decimal BuildVolumeZ { get; set; }
    public Guid PrintingTechnologyId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CurrentStatus { get; set; } // e.g., "Idle", "Printing", "Maintenance", "Error"
    public string? Location { get; set; }
    
    // Navigation properties
    public PrintingTechnology PrintingTechnology { get; set; } = null!;
    public ICollection<SlicingProfile> SlicingProfiles { get; set; } = new List<SlicingProfile>();
    public ICollection<PrintJob> PrintJobs { get; set; } = new List<PrintJob>();
}

