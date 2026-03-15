using Maba.Domain.Common;

namespace Maba.Domain.Orders;

public class InvoiceStatus : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

