using Maba.Domain.Catalog;
using Maba.Domain.Common;

namespace Maba.Domain.Pricing;

public class PriceListItem : BaseEntity
{
    public Guid PriceListId { get; set; }
    public Guid ItemId { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public decimal Price { get; set; }
    public int MinQuantity { get; set; } = 1;

    public PriceList PriceList { get; set; } = null!;
    public Item Item { get; set; } = null!;
    public UnitOfMeasure UnitOfMeasure { get; set; } = null!;
}
