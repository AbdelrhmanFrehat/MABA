using Maba.Domain.Common;
using Maba.Domain.Crm;
using Maba.Domain.Lookups;

namespace Maba.Domain.Pricing;

public class PriceList : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public Guid PriceListTypeId { get; set; }
    public string Currency { get; set; } = "ILS";
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }

    public LookupValue PriceListType { get; set; } = null!;
    public ICollection<PriceListItem> Items { get; set; } = new List<PriceListItem>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
