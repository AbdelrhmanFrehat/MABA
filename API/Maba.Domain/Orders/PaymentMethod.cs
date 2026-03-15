using Maba.Domain.Common;

namespace Maba.Domain.Orders;

public class PaymentMethod : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

