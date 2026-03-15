using MediatR;
using Maba.Application.Features.Printing.PrintQualityProfiles.DTOs;

namespace Maba.Application.Features.Printing.PrintQualityProfiles.Commands;

public class UpdatePrintQualityProfileCommand : IRequest<PrintQualityProfileDto?>
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public decimal LayerHeightMm { get; set; }
    public string SpeedCategory { get; set; } = "Normal";
    public decimal PriceMultiplier { get; set; } = 1.0m;
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}
