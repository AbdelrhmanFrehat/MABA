using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Accounting;
using Maba.Domain.CommercialReturns;
using Maba.Domain.Finance;
using Maba.Domain.Lookups;
using Maba.Domain.Orders;
using Maba.Infrastructure.Data;

namespace Maba.Infrastructure.Services;

/// <summary>
/// Creates balanced double-entry journal entries for commercial documents.
/// Account codes are resolved by the seeded system accounts:
///   1000 = Cash, 1010 = Bank, 1200 = Accounts Receivable,
///   4000 = Sales Revenue, 6000 = Operating Expenses, 2000 = Accounts Payable
/// </summary>
public class AccountingPostingService : IAccountingPostingService
{
    private readonly ApplicationDbContext _db;
    private readonly IDocumentNumberService _docNumbers;

    public AccountingPostingService(ApplicationDbContext db, IDocumentNumberService docNumbers)
    {
        _db = db;
        _docNumbers = docNumbers;
    }

    // ── Invoice posting ─────────────────────────────────────────────────────

    public async Task PostInvoiceAsync(Guid invoiceId, Guid postedByUserId, CancellationToken cancellationToken = default)
    {
        var invoice = await _db.Set<Invoice>()
            .Include(x => x.Order)
            .Include(x => x.InvoiceStatus)
            .FirstOrDefaultAsync(x => x.Id == invoiceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice {invoiceId} not found.");

        if (invoice.IsPosted)
            throw new InvalidOperationException($"Invoice {invoice.InvoiceNumber} is already posted.");

        var arAccount  = await RequireAccountByCodeAsync("1200", cancellationToken); // AR
        var revAccount = await RequireAccountByCodeAsync("4000", cancellationToken); // Revenue
        var typeId     = await RequireJournalEntryTypeIdAsync("Sales", cancellationToken);
        var periodId   = await FindFiscalPeriodIdAsync(invoice.IssueDate, cancellationToken);
        var entryNo    = await _docNumbers.GenerateNextAsync("JournalEntry", cancellationToken);

        var entry = BuildBalancedEntry(
            entryNumber:         entryNo,
            entryDate:           invoice.IssueDate,
            fiscalPeriodId:      periodId,
            typeId:              typeId,
            description:         $"Sales invoice {invoice.InvoiceNumber}",
            sourceDocType:       "Invoice",
            sourceDocId:         invoice.Id,
            sourceDocNumber:     invoice.InvoiceNumber,
            currency:            invoice.Currency,
            postedByUserId:      postedByUserId,
            debitAccountId:      arAccount.Id,
            creditAccountId:     revAccount.Id,
            amount:              invoice.Total,
            debitDescription:    $"AR – {invoice.InvoiceNumber}",
            creditDescription:   $"Revenue – {invoice.InvoiceNumber}"
        );

        _db.Set<JournalEntry>().Add(entry);

        // Update account running balances
        arAccount.CurrentBalance  += invoice.Total;
        revAccount.CurrentBalance += invoice.Total;

        // Mark invoice as posted
        invoice.IsPosted       = true;
        invoice.PostedAt       = DateTime.UtcNow;
        invoice.PostedByUserId = postedByUserId;
        invoice.JournalEntryId = entry.Id;

        await _db.SaveChangesAsync(cancellationToken);
    }

    // ── Payment posting ──────────────────────────────────────────────────────

    public async Task PostPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _db.Set<Payment>()
            .Include(x => x.PaymentMethod)
            .FirstOrDefaultAsync(x => x.Id == paymentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Payment {paymentId} not found.");

        // Determine Cash vs Bank account from payment method
        var cashCode = payment.PaymentMethod.Key == "BankTransfer" ? "1010" : "1000";
        var cashAccount = await RequireAccountByCodeAsync(cashCode, cancellationToken);
        var arAccount   = await RequireAccountByCodeAsync("1200", cancellationToken);
        var typeId      = await RequireJournalEntryTypeIdAsync("PaymentReceived", cancellationToken);
        var periodId    = await FindFiscalPeriodIdAsync(payment.PaidAt, cancellationToken);
        var entryNo     = await _docNumbers.GenerateNextAsync("JournalEntry", cancellationToken);

        var description = $"Payment {payment.RefNo ?? payment.Id.ToString()[..8]}";
        var entry = BuildBalancedEntry(
            entryNumber:         entryNo,
            entryDate:           payment.PaidAt,
            fiscalPeriodId:      periodId,
            typeId:              typeId,
            description:         description,
            sourceDocType:       "Payment",
            sourceDocId:         payment.Id,
            sourceDocNumber:     payment.RefNo,
            currency:            payment.Currency,
            postedByUserId:      null,
            debitAccountId:      cashAccount.Id,
            creditAccountId:     arAccount.Id,
            amount:              payment.Amount,
            debitDescription:    $"Cash received – {description}",
            creditDescription:   $"AR cleared – {description}"
        );

        _db.Set<JournalEntry>().Add(entry);

        cashAccount.CurrentBalance += payment.Amount;
        arAccount.CurrentBalance   -= payment.Amount;

        // Update invoice paid status if linked
        if (payment.InvoiceId.HasValue)
            await UpdateInvoicePaidStatusAsync(payment.InvoiceId.Value, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
    }

    // ── Sales return posting ─────────────────────────────────────────────────

    public async Task PostSalesReturnAsync(Guid salesReturnId, Guid postedByUserId, CancellationToken cancellationToken = default)
    {
        var ret = await _db.Set<SalesReturn>()
            .FirstOrDefaultAsync(x => x.Id == salesReturnId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sales return {salesReturnId} not found.");

        if (ret.Total <= 0) return; // nothing to reverse

        var revAccount = await RequireAccountByCodeAsync("4000", cancellationToken); // Revenue
        var arAccount  = await RequireAccountByCodeAsync("1200", cancellationToken); // AR
        var typeId     = await RequireJournalEntryTypeIdAsync("SalesReturn", cancellationToken);
        var periodId   = await FindFiscalPeriodIdAsync(ret.ReturnDate, cancellationToken);
        var entryNo    = await _docNumbers.GenerateNextAsync("JournalEntry", cancellationToken);

        // Reversal: DR Revenue / CR AR (opposite of original invoice entry)
        var entry = BuildBalancedEntry(
            entryNumber:         entryNo,
            entryDate:           ret.ReturnDate,
            fiscalPeriodId:      periodId,
            typeId:              typeId,
            description:         $"Sales return {ret.ReturnNumber}",
            sourceDocType:       "SalesReturn",
            sourceDocId:         ret.Id,
            sourceDocNumber:     ret.ReturnNumber,
            currency:            ret.Currency,
            postedByUserId:      postedByUserId,
            debitAccountId:      revAccount.Id,
            creditAccountId:     arAccount.Id,
            amount:              ret.Total,
            debitDescription:    $"Revenue reversed – {ret.ReturnNumber}",
            creditDescription:   $"AR credited – {ret.ReturnNumber}"
        );

        _db.Set<JournalEntry>().Add(entry);

        revAccount.CurrentBalance -= ret.Total;
        arAccount.CurrentBalance  -= ret.Total;

        await _db.SaveChangesAsync(cancellationToken);
    }

    // ── Expense posting ──────────────────────────────────────────────────────

    public async Task PostExpenseAsync(Guid expenseId, Guid postedByUserId, CancellationToken cancellationToken = default)
    {
        var expense = await _db.Set<Expense>()
            .FirstOrDefaultAsync(x => x.Id == expenseId, cancellationToken)
            ?? throw new KeyNotFoundException($"Expense {expenseId} not found.");

        if (expense.IsPosted)
            throw new InvalidOperationException($"Expense {expenseId} is already posted.");

        // Use the expense's explicit account if set, otherwise default operating expenses
        var expenseAccountId = expense.AccountId;
        if (!expenseAccountId.HasValue)
        {
            var defaultExpAccount = await RequireAccountByCodeAsync("6000", cancellationToken);
            expenseAccountId = defaultExpAccount.Id;
        }

        // Credit cash unless a supplier is attached (then credit AP)
        var creditCode    = expense.SupplierId.HasValue ? "2000" : "1000";
        var expAccount    = await _db.Set<Account>().FindAsync(new object[] { expenseAccountId.Value }, cancellationToken)
                           ?? throw new InvalidOperationException("Expense account not found.");
        var creditAccount = await RequireAccountByCodeAsync(creditCode, cancellationToken);

        var typeId   = await RequireJournalEntryTypeIdAsync("Expense", cancellationToken);
        var periodId = await FindFiscalPeriodIdAsync(expense.SpentAt, cancellationToken);
        var entryNo  = await _docNumbers.GenerateNextAsync("JournalEntry", cancellationToken);

        var desc = expense.DescriptionEn ?? expense.DescriptionAr ?? "Expense";
        var entry = BuildBalancedEntry(
            entryNumber:         entryNo,
            entryDate:           expense.SpentAt,
            fiscalPeriodId:      periodId,
            typeId:              typeId,
            description:         desc,
            sourceDocType:       "Expense",
            sourceDocId:         expense.Id,
            sourceDocNumber:     null,
            currency:            expense.Currency,
            postedByUserId:      postedByUserId,
            debitAccountId:      expAccount.Id,
            creditAccountId:     creditAccount.Id,
            amount:              expense.Amount,
            debitDescription:    $"Expense – {desc}",
            creditDescription:   $"Cash/Payable – {desc}"
        );

        _db.Set<JournalEntry>().Add(entry);

        expAccount.CurrentBalance    += expense.Amount;
        creditAccount.CurrentBalance -= expense.Amount;

        expense.IsPosted       = true;
        expense.JournalEntryId = entry.Id;
        expense.UpdatedAt      = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private JournalEntry BuildBalancedEntry(
        string entryNumber, DateTime entryDate, Guid? fiscalPeriodId,
        Guid typeId, string description,
        string? sourceDocType, Guid? sourceDocId, string? sourceDocNumber,
        string currency, Guid? postedByUserId,
        Guid debitAccountId, Guid creditAccountId,
        decimal amount, string debitDescription, string creditDescription)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Journal entry amount must be positive.");

        var entry = new JournalEntry
        {
            Id                   = Guid.NewGuid(),
            EntryNumber          = entryNumber,
            EntryDate            = entryDate,
            FiscalPeriodId       = fiscalPeriodId,
            JournalEntryTypeId   = typeId,
            Description          = description,
            SourceDocumentType   = sourceDocType,
            SourceDocumentId     = sourceDocId,
            SourceDocumentNumber = sourceDocNumber,
            TotalDebit           = amount,
            TotalCredit          = amount,
            IsPosted             = true,
            PostedAt             = DateTime.UtcNow,
            PostedByUserId       = postedByUserId,
            CreatedByUserId      = postedByUserId ?? Guid.Empty
        };

        entry.Lines = new List<JournalEntryLine>
        {
            new JournalEntryLine
            {
                Id             = Guid.NewGuid(),
                JournalEntryId = entry.Id,
                AccountId      = debitAccountId,
                Debit          = amount,
                Credit         = 0,
                Description    = debitDescription,
                SortOrder      = 1
            },
            new JournalEntryLine
            {
                Id             = Guid.NewGuid(),
                JournalEntryId = entry.Id,
                AccountId      = creditAccountId,
                Debit          = 0,
                Credit         = amount,
                Description    = creditDescription,
                SortOrder      = 2
            }
        };

        return entry;
    }

    private async Task<Account> RequireAccountByCodeAsync(string code, CancellationToken ct)
    {
        return await _db.Set<Account>()
            .FirstOrDefaultAsync(a => a.Code == code, ct)
            ?? throw new InvalidOperationException($"System account '{code}' not found. Ensure the database seeder has run.");
    }

    private async Task<Guid> RequireJournalEntryTypeIdAsync(string key, CancellationToken ct)
    {
        var lv = await _db.Set<LookupValue>()
            .FirstOrDefaultAsync(v => v.Key == key, ct)
            ?? throw new InvalidOperationException($"LookupValue '{key}' not found.");
        return lv.Id;
    }

    private async Task<Guid?> FindFiscalPeriodIdAsync(DateTime date, CancellationToken ct)
    {
        var period = await _db.Set<FiscalPeriod>()
            .Where(p => !p.IsClosed && p.StartDate <= date && date <= p.EndDate)
            .OrderBy(p => p.StartDate)
            .FirstOrDefaultAsync(ct);
        return period?.Id;
    }

    private async Task UpdateInvoicePaidStatusAsync(Guid invoiceId, CancellationToken ct)
    {
        var invoice = await _db.Set<Invoice>()
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == invoiceId, ct);
        if (invoice == null) return;

        var paid = invoice.Payments.Where(p => !p.IsRefunded).Sum(p => p.Amount);

        string targetKey;
        if (paid >= invoice.Total)
            targetKey = "Paid";
        else if (paid > 0)
            targetKey = "PartiallyPaid";
        else
            return;

        // Try PartiallyPaid first; fall back to Issued if not seeded
        var status = await _db.Set<InvoiceStatus>().FirstOrDefaultAsync(s => s.Key == targetKey, ct)
                  ?? (targetKey == "PartiallyPaid"
                      ? await _db.Set<InvoiceStatus>().FirstOrDefaultAsync(s => s.Key == "Issued", ct)
                      : null);

        if (status != null && invoice.InvoiceStatusId != status.Id)
        {
            invoice.InvoiceStatusId = status.Id;
            invoice.UpdatedAt = DateTime.UtcNow;
        }
    }
}
