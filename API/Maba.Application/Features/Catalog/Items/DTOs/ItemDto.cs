using Maba.Application.Features.Catalog.Items.Commands;

namespace Maba.Application.Features.Catalog.Items.DTOs;

public class ItemDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? GeneralDescriptionEn { get; set; }
    public string? GeneralDescriptionAr { get; set; }
    public Guid ItemStatusId { get; set; }
    public string ItemStatusKey { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "ILS";
    public Guid? BrandId { get; set; }
    public string? BrandNameEn { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryNameEn { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public decimal? TaxRate { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsNew { get; set; }
    public bool IsOnSale { get; set; }
    public int? MinOrderQuantity { get; set; }
    public int? MaxOrderQuantity { get; set; }
    public int? WarrantyPeriodMonths { get; set; }
    public string? MetaTitleEn { get; set; }
    public string? MetaTitleAr { get; set; }
    public string? MetaDescriptionEn { get; set; }
    public string? MetaDescriptionAr { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewsCount { get; set; }
    public int ViewsCount { get; set; }
    public List<Guid> TagIds { get; set; } = new();
    public InventoryDto? Inventory { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public List<ItemMediaDto> MediaAssets { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class InventoryDto
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public int QuantityAvailable { get; set; }
    public int QuantityOnOrder { get; set; }
    public int ReorderLevel { get; set; }
    public decimal? CostPerUnit { get; set; }
    public DateTime? LastStockInAt { get; set; }
    public DateTime? LastStockOutAt { get; set; }
}

