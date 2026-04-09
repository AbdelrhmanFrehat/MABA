using Maba.Domain.Common;
using Maba.Domain.Crm;
using Maba.Domain.Sales;
using Maba.Domain.Users;

namespace Maba.Domain.Orders;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty; // ORD-YYYY-XXX
    public bool IsStorefrontOrder { get; set; } = true;
    public Guid UserId { get; set; }
    public Guid OrderStatusId { get; set; }

    // ── Commercial pipeline linkage ──────────────────────────────────────────
    /// <summary>ERP Customer — populated for service-originated orders.</summary>
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    /// <summary>Quotation this order was converted from (if any).</summary>
    public Guid? SourceQuotationId { get; set; }
    public Quotation? SourceQuotation { get; set; }

    /// <summary>Originating service request ID (set when converted from a request/quotation).</summary>
    public Guid? SourceRequestId { get; set; }

    /// <summary>'project' | 'cnc' | 'laser' | 'print3d' | 'design' | 'designCad'</summary>
    public string? SourceRequestType { get; set; }

    /// <summary>Cached reference number for display.</summary>
    public string? SourceRequestReference { get; set; }
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

