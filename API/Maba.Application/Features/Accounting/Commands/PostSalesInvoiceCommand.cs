using MediatR;

namespace Maba.Application.Features.Accounting.Commands;

public class PostSalesInvoiceCommand : IRequest<PostSalesInvoiceResult>
{
    public Guid InvoiceId { get; set; }
    public Guid PostedByUserId { get; set; }
}

public class PostSalesInvoiceResult
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid JournalEntryId { get; set; }
    public string JournalEntryNumber { get; set; } = string.Empty;
    public DateTime PostedAt { get; set; }
}
