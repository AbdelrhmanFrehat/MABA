using Maba.Domain.Common;

namespace Maba.Domain.Orders;

public class Payment : BaseEntity
{
    public Guid? OrderId { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ILS";
    public DateTime PaidAt { get; set; }
    public string? RefNo { get; set; }
    public string? TransactionId { get; set; } // From payment gateway
    public string? GatewayResponse { get; set; } // JSON response from gateway
    public bool IsRefunded { get; set; } = false;
    public DateTime? RefundedAt { get; set; }
    public decimal? RefundedAmount { get; set; }
    
    // Navigation properties
    public Order? Order { get; set; }
    public Invoice? Invoice { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = null!;
}

