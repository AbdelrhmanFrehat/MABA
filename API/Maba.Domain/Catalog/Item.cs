using Maba.Domain.Common;
using Maba.Domain.Crm;
using Maba.Domain.Machines;
using Maba.Domain.Pricing;
using Maba.Domain.Tax;

namespace Maba.Domain.Catalog;

public class Item : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? GeneralDescriptionEn { get; set; }
    public string? GeneralDescriptionAr { get; set; }
    public Guid ItemStatusId { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string Currency { get; set; } = "ILS";
    public decimal? Weight { get; set; } // in kg
    public string? Dimensions { get; set; } // JSON or string like "10x20x30"
    public decimal? TaxRate { get; set; }
    public Guid? BaseUnitOfMeasureId { get; set; }
    public decimal? WholesalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public bool IsTaxable { get; set; } = true;
    public Guid? AccountId { get; set; }
    public Guid? DefaultTaxConfigurationId { get; set; }
    public bool IsFeatured { get; set; } = false;
    public bool IsNew { get; set; } = false;
    public bool IsOnSale { get; set; } = false;
    public int? MinOrderQuantity { get; set; }
    public int? MaxOrderQuantity { get; set; }
    public int? WarrantyPeriodMonths { get; set; }
    public string? MetaTitleEn { get; set; }
    public string? MetaTitleAr { get; set; }
    public string? MetaDescriptionEn { get; set; }
    public string? MetaDescriptionAr { get; set; }
    public Guid? BrandId { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewsCount { get; set; }
    public int ViewsCount { get; set; }
    
    // Navigation properties
    public ItemStatus ItemStatus { get; set; } = null!;
    public Brand? Brand { get; set; }
    public Category? Category { get; set; }
    public ICollection<ItemTag> ItemTags { get; set; } = new List<ItemTag>();
    public ICollection<ItemSection> ItemSections { get; set; } = new List<ItemSection>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public Inventory? Inventory { get; set; }
    public ICollection<ItemMachineLink> ItemMachineLinks { get; set; } = new List<ItemMachineLink>();
    public UnitOfMeasure? BaseUnitOfMeasure { get; set; }
    public TaxConfiguration? DefaultTaxConfiguration { get; set; }
    public ICollection<ItemUnit> ItemUnits { get; set; } = new List<ItemUnit>();
    public ICollection<SupplierItemPrice> SupplierItemPrices { get; set; } = new List<SupplierItemPrice>();
    public ICollection<PriceListItem> PriceListItems { get; set; } = new List<PriceListItem>();
}

