using Maba.Domain.Common;

namespace Maba.Domain.CommercialReturns;

public class PurchaseReturnLine : BaseEntity
{
    public Guid PurchaseReturnId { get; set; }
    public Guid? PurchaseInvoiceLineId { get; set; }
    public Guid ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? ItemSku { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public string? Notes { get; set; }

    public PurchaseReturn PurchaseReturn { get; set; } = null!;
}
