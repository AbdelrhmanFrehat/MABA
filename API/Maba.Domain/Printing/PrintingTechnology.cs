using Maba.Domain.Common;

namespace Maba.Domain.Printing;

public class PrintingTechnology : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<Printer> Printers { get; set; } = new List<Printer>();
    public ICollection<SlicingProfile> SlicingProfiles { get; set; } = new List<SlicingProfile>();
}

