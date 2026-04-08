using Maba.Domain.Common;

namespace Maba.Domain.CommercialReturns;

public class SalesReturnLine : BaseEntity
{
    public Guid SalesReturnId { get; set; }
    public Guid? SalesInvoiceLineId { get; set; }
    public Guid ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? ItemSku { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public string? Notes { get; set; }

    public SalesReturn SalesReturn { get; set; } = null!;
}
