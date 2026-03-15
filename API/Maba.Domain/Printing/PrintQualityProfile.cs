using Maba.Domain.Common;

namespace Maba.Domain.Printing;

public class PrintQualityProfile : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public decimal LayerHeightMm { get; set; }
    public string SpeedCategory { get; set; } = "Normal"; // Fast, Normal, Slow
    public decimal PriceMultiplier { get; set; } = 1.0m;
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}
