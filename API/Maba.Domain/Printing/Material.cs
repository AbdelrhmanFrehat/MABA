using Maba.Domain.Common;

namespace Maba.Domain.Printing;

public class Material : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public decimal PricePerGram { get; set; }
    public decimal Density { get; set; }
    public string? Color { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal StockQuantity { get; set; } = 0; // in grams
    
    // Navigation properties
    public ICollection<SlicingProfile> SlicingProfiles { get; set; } = new List<SlicingProfile>();
    public ICollection<MaterialColor> AvailableColors { get; set; } = new List<MaterialColor>();
}

