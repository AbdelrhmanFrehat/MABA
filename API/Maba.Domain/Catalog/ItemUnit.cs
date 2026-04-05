using Maba.Domain.Common;

namespace Maba.Domain.Catalog;

public class ItemUnit : BaseEntity
{
    public Guid ItemId { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public decimal ConversionToBase { get; set; }
    public string? Barcode { get; set; }
    public bool IsDefault { get; set; }
    public bool IsPurchaseDefault { get; set; }

    public Item Item { get; set; } = null!;
    public UnitOfMeasure UnitOfMeasure { get; set; } = null!;
}
