namespace Maba.Application.Features.Printing.PrintQualityProfiles.DTOs;

public class PrintQualityProfileDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public decimal LayerHeightMm { get; set; }
    public string SpeedCategory { get; set; } = "Normal";
    public decimal PriceMultiplier { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
