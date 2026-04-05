using Maba.Domain.Common;
using Maba.Domain.Users;

namespace Maba.Domain.Orders;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty; // ORD-YYYY-XXX
    public bool IsStorefrontOrder { get; set; } = true;
    public Guid UserId { get; set; }
    public Guid OrderStatusId { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = "ILS";
    public Guid? ShippingMethodId { get; set; }
    public string? ShippingAddress { get; set; } // JSON
    public string? BillingAddress { get; set; } // JSON
    public string? Notes { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? TrackingNumber { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public OrderStatus OrderStatus { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public PaymentPlan? PaymentPlan { get; set; }
}

