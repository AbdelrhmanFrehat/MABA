using Maba.Domain.Common;

namespace Maba.Domain.CommercialReturns;

public class SalesReturn : BaseEntity
{
    public string ReturnNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string StatusKey { get; set; } = "draft";
    public string StatusName { get; set; } = "Draft";
    public string? StatusColor { get; set; }
    public Guid? SalesInvoiceId { get; set; }
    public string? SalesInvoiceNumber { get; set; }
    public string? ReturnReasonLookupId { get; set; }
    public string? ReturnReasonName { get; set; }
    public DateTime ReturnDate { get; set; }
    public string Currency { get; set; } = "ILS";
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public bool IsPosted { get; set; }
    public bool RestockItems { get; set; } = true;
    public string? Notes { get; set; }
    public Guid? WarehouseId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public ICollection<SalesReturnLine> Lines { get; set; } = new List<SalesReturnLine>();
}
