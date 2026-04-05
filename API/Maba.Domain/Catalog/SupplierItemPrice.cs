using Maba.Domain.Common;
using Maba.Domain.Crm;

namespace Maba.Domain.Catalog;

public class SupplierItemPrice : BaseEntity
{
    public Guid SupplierId { get; set; }
    public Guid ItemId { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "ILS";
    public int MinQuantity { get; set; } = 1;
    public int? LeadTimeDays { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public Supplier Supplier { get; set; } = null!;
    public Item Item { get; set; } = null!;
    public UnitOfMeasure UnitOfMeasure { get; set; } = null!;
}
