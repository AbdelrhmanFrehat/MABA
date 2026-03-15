using Maba.Domain.Common;

namespace Maba.Domain.Orders;

public class Installment : BaseEntity
{
    public Guid PaymentPlanId { get; set; }
    public int Seq { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public Guid InstallmentStatusId { get; set; }
    public DateTime? PaidAt { get; set; }
    public Guid? PaymentId { get; set; } // Link to payment when paid
    
    // Navigation properties
    public PaymentPlan PaymentPlan { get; set; } = null!;
    public InstallmentStatus InstallmentStatus { get; set; } = null!;
    public Payment? Payment { get; set; }
}

