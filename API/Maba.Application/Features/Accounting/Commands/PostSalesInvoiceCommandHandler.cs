using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Accounting;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Accounting.Commands;

public class PostSalesInvoiceCommandHandler : IRequestHandler<PostSalesInvoiceCommand, PostSalesInvoiceResult>
{
    private readonly IAccountingPostingService _posting;
    private readonly IApplicationDbContext _context;

    public PostSalesInvoiceCommandHandler(IAccountingPostingService posting, IApplicationDbContext context)
    {
        _posting = posting;
        _context = context;
    }

    public async Task<PostSalesInvoiceResult> Handle(PostSalesInvoiceCommand request, CancellationToken cancellationToken)
    {
        await _posting.PostInvoiceAsync(request.InvoiceId, request.PostedByUserId, cancellationToken);

        var invoice = await _context.Set<Invoice>()
            .FirstOrDefaultAsync(x => x.Id == request.InvoiceId, cancellationToken)
            ?? throw new KeyNotFoundException("Invoice not found after posting.");

        var je = invoice.JournalEntryId.HasValue
            ? await _context.Set<JournalEntry>().FindAsync(new object[] { invoice.JournalEntryId.Value }, cancellationToken)
            : null;

        return new PostSalesInvoiceResult
        {
            InvoiceId           = invoice.Id,
            InvoiceNumber       = invoice.InvoiceNumber,
            JournalEntryId      = je?.Id ?? Guid.Empty,
            JournalEntryNumber  = je?.EntryNumber ?? string.Empty,
            PostedAt            = invoice.PostedAt ?? DateTime.UtcNow
        };
    }
}
