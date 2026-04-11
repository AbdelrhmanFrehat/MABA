using Maba.Domain.Common;

namespace Maba.Domain.Orders;

public class Invoice : BaseEntity
{
    public Guid OrderId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = "ILS";
    public Guid InvoiceStatusId { get; set; }
    public string? PdfUrl { get; set; }

    // Accounting posting state
    public bool IsPosted { get; set; } = false;
    public DateTime? PostedAt { get; set; }
    public Guid? PostedByUserId { get; set; }
    public Guid? JournalEntryId { get; set; }

    // Navigation properties
    public Order Order { get; set; } = null!;
    public InvoiceStatus InvoiceStatus { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

