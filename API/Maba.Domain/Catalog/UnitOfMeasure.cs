using Maba.Domain.Common;

namespace Maba.Domain.Catalog;

public class UnitOfMeasure : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public bool IsBase { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UnitConversion> FromConversions { get; set; } = new List<UnitConversion>();
    public ICollection<UnitConversion> ToConversions { get; set; } = new List<UnitConversion>();
    public ICollection<ItemUnit> ItemUnits { get; set; } = new List<ItemUnit>();
}
