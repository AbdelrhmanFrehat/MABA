using System.Text.Json;
using System.Text.Json.Serialization;

namespace Maba.Application.Features.Orders.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public Guid OrderStatusId { get; set; }
    public string OrderStatusKey { get; set; } = string.Empty;
    public OrderStatusDto? Status { get; set; }
    
    public decimal SubTotal { get; set; }
    
    public decimal TaxAmount { get; set; }
    
    public decimal ShippingCost { get; set; }
    [JsonPropertyName("shippingAmount")]
    public decimal ShippingAmount { get => ShippingCost; set => ShippingCost = value; }
    
    public decimal DiscountAmount { get; set; }
    
    public decimal Total { get; set; }
    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get => Total; set => Total = value; }
    
    public string Currency { get; set; } = "ILS";
    public string? PaymentStatus { get; set; }
    
    // Store JSON strings internally
    [JsonIgnore]
    public string? ShippingAddressJson { get; set; }
    [JsonIgnore]
    public string? BillingAddressJson { get; set; }
    
    // Expose as objects
    public AddressDto? ShippingAddress 
    { 
        get => AddressDto.FromJson(ShippingAddressJson);
        set => ShippingAddressJson = value != null ? JsonSerializer.Serialize(value) : null;
    }
    
    public AddressDto? BillingAddress 
    { 
        get => AddressDto.FromJson(BillingAddressJson);
        set => BillingAddressJson = value != null ? JsonSerializer.Serialize(value) : null;
    }
    
    public string? Notes { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? TrackingNumber { get; set; }
    
    // Keep both names
    public List<OrderItemDto> OrderItems { get; set; } = new();
    [JsonPropertyName("items")]
    public List<OrderItemDto> Items { get => OrderItems; set => OrderItems = value; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class AddressDto
{
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = string.Empty;
    
    public static AddressDto? FromJson(string? json)
    {
        if (string.IsNullOrEmpty(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<AddressDto>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
        }
        catch
        {
            return null;
        }
    }
}

public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid? ItemId { get; set; }
    public string? ItemNameEn { get; set; }
    public string? ItemNameAr { get; set; }
    public string? ItemSku { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    
    // Alias for frontend
    [JsonPropertyName("subtotal")]
    public decimal Subtotal { get => UnitPrice * Quantity; }
    [JsonPropertyName("total")]
    public decimal Total { get => LineTotal; set => LineTotal = value; }
    
    public string? MediaAssetId { get; set; }
    public string? MediaAssetUrl { get; set; }
    public string? MetaJson { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class InvoiceDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = "ILS";
    public Guid InvoiceStatusId { get; set; }
    public string InvoiceStatusKey { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public string PaymentMethodKey { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ILS";
    public DateTime PaidAt { get; set; }
    public string? RefNo { get; set; }
    public string? TransactionId { get; set; }
    public bool IsRefunded { get; set; }
    public DateTime? RefundedAt { get; set; }
    public decimal? RefundedAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaymentPlanDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal DownPayment { get; set; }
    public int InstallmentsCount { get; set; }
    public string InstallmentFrequency { get; set; } = string.Empty;
    public decimal InterestRate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public List<InstallmentDto> Installments { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class InstallmentDto
{
    public Guid Id { get; set; }
    public Guid PaymentPlanId { get; set; }
    public int Seq { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public Guid InstallmentStatusId { get; set; }
    public string InstallmentStatusKey { get; set; } = string.Empty;
    public Guid? PaymentId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
