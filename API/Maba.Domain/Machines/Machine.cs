using Maba.Domain.Common;

namespace Maba.Domain.Machines;

public class Machine : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
    public Guid? ImageId { get; set; } // MediaAsset reference
    public Guid? ManualId { get; set; } // MediaAsset reference (PDF/document)
    public int? WarrantyMonths { get; set; }
    public string? Location { get; set; }
    public decimal? PurchasePrice { get; set; }
    public DateTime? PurchaseDate { get; set; }
    
    // Navigation properties
    public ICollection<MachinePart> Parts { get; set; } = new List<MachinePart>();
    public ICollection<ItemMachineLink> ItemMachineLinks { get; set; } = new List<ItemMachineLink>();
}

