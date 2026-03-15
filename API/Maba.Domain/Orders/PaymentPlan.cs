using Maba.Domain.Common;

namespace Maba.Domain.Orders;

public class PaymentPlan : BaseEntity
{
    public Guid OrderId { get; set; }
    public decimal DownPayment { get; set; }
    public int InstallmentsCount { get; set; }
    public string InstallmentFrequency { get; set; } = string.Empty; // Monthly, Weekly, etc.
    public decimal InterestRate { get; set; }
    public decimal TotalAmount { get; set; } // Calculated: DownPayment + Sum of Installments
    public decimal RemainingAmount { get; set; } // Calculated
    
    // Navigation properties
    public Order Order { get; set; } = null!;
    public ICollection<Installment> Installments { get; set; } = new List<Installment>();
}

