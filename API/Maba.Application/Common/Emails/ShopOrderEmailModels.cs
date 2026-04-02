namespace Maba.Application.Common.Emails;

/// <summary>Data for the customer order confirmation email (shop checkout).</summary>
public sealed class ShopOrderConfirmationEmailModel
{
    public string OrderNumber { get; init; } = string.Empty;
    public DateTime OrderDateUtc { get; init; }
    public string? CustomerName { get; init; }
    public string? PaymentMethod { get; init; }
    public string? ShippingMethod { get; init; }
    public IReadOnlyList<ShopOrderEmailLineItem> Items { get; init; } = Array.Empty<ShopOrderEmailLineItem>();
    public decimal SubTotal { get; init; }
    public decimal Shipping { get; init; }
    public decimal Tax { get; init; }
    public decimal Discount { get; init; }
    public decimal Total { get; init; }
    public string Currency { get; init; } = "ILS";
    public string? ShippingAddressLinesHtml { get; init; }
    public string? BillingAddressLinesHtml { get; init; }
    public string ViewOrderUrl { get; init; } = string.Empty;
    public string PublicSiteUrl { get; init; } = "https://mabasol.com";
}

public sealed class ShopOrderEmailLineItem
{
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}

/// <summary>Data for the order shipped notification email.</summary>
public sealed class ShopOrderShippedEmailModel
{
    public string OrderNumber { get; init; } = string.Empty;
    public string? TrackingNumber { get; init; }
    public string? Carrier { get; init; }
    public DateTime? ShippedDateUtc { get; init; }
    public DateTime? EstimatedDeliveryUtc { get; init; }
    public string ViewOrderUrl { get; init; } = string.Empty;
    /// <summary>If set (e.g. carrier tracking URL), primary CTA can point here.</summary>
    public string? TrackingUrl { get; init; }
    public string PublicSiteUrl { get; init; } = "https://mabasol.com";
}
