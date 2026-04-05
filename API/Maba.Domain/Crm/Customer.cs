using Maba.Domain.Catalog;
using Maba.Domain.Common;
using Maba.Domain.Lookups;
using Maba.Domain.Pricing;
using Maba.Domain.Users;

namespace Maba.Domain.Crm;

public class Customer : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Phone2 { get; set; }
    public Guid CustomerTypeId { get; set; }
    public string? TaxNumber { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Currency { get; set; } = "ILS";
    public string? BillingAddress { get; set; }
    public string? ShippingAddress { get; set; }
    public string? ContactPerson { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? UserId { get; set; }
    public Guid? PriceListId { get; set; }

    public LookupValue CustomerType { get; set; } = null!;
    public User? User { get; set; }
    public PriceList? PriceList { get; set; }
}
