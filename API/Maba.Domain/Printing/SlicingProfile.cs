using Maba.Domain.Common;

namespace Maba.Domain.Printing;

public class SlicingProfile : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public Guid PrintingTechnologyId { get; set; }
    public decimal LayerHeightMm { get; set; }
    public decimal InfillPercent { get; set; }
    public bool SupportsEnabled { get; set; }
    public Guid MaterialId { get; set; }
    public Guid? PrinterId { get; set; }
    public bool IsDefault { get; set; } = false;
    public string? TemperatureSettings { get; set; } // JSON string for nozzle/bed temperatures
    
    // Navigation properties
    public PrintingTechnology PrintingTechnology { get; set; } = null!;
    public Material Material { get; set; } = null!;
    public Printer? Printer { get; set; }
    public ICollection<SlicingJob> SlicingJobs { get; set; } = new List<SlicingJob>();
}

