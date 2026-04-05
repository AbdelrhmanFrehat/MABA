using Maba.Domain.Common;

namespace Maba.Domain.Catalog;

public class UnitConversion : BaseEntity
{
    public Guid FromUnitId { get; set; }
    public Guid ToUnitId { get; set; }
    public decimal ConversionFactor { get; set; }

    public UnitOfMeasure FromUnit { get; set; } = null!;
    public UnitOfMeasure ToUnit { get; set; } = null!;
}
