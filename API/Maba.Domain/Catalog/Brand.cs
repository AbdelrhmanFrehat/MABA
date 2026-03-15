using Maba.Domain.Common;

namespace Maba.Domain.Catalog;

public class Brand : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Guid? LogoId { get; set; } // MediaAsset reference
    public string? WebsiteUrl { get; set; }
    public string? Country { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    
    // Navigation properties
    public ICollection<Item> Items { get; set; } = new List<Item>();
}

