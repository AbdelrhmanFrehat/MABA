namespace Maba.Application.Features.Catalog.Brands.DTOs;

public class BrandDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Guid? LogoId { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? Country { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

