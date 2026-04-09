using Maba.Domain.Common;

namespace Maba.Domain.Sales;

/// <summary>Line item within a quotation.</summary>
public class QuotationItem : BaseEntity
{
    public Guid QuotationId { get; set; }
    public Quotation Quotation { get; set; } = null!;

    public int LineNumber { get; set; }

    /// <summary>Free-text description of the service/product being quoted.</summary>
    public string Description { get; set; } = string.Empty;

    public decimal Quantity { get; set; } = 1;
    public string Unit { get; set; } = "pcs";

    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }

    public string? Notes { get; set; }
}
