using Maba.Domain.Catalog;
using Maba.Domain.Common;
using Maba.Domain.Lookups;

namespace Maba.Domain.Crm;

public class Supplier : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Phone2 { get; set; }
    public Guid? SupplierTypeId { get; set; }
    public string? TaxNumber { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Currency { get; set; } = "ILS";
    public string? Address { get; set; }
    public string? ContactPerson { get; set; }
    public int PaymentTermDays { get; set; } = 30;
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public LookupValue? SupplierType { get; set; }
    public ICollection<SupplierItemPrice> SupplierItemPrices { get; set; } = new List<SupplierItemPrice>();
}
