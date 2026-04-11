using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Accounting;
using Maba.Domain.Orders;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public ReportsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("Sales")]
    public async Task<ActionResult<IEnumerable<SalesReportRowDto>>> GetSalesReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var ordersQuery = _context.Set<Order>()
            .Include(x => x.User)
            .Include(x => x.Payments)
            .Include(x => x.Invoices)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            var from = fromDate.Value.Date;
            ordersQuery = ordersQuery.Where(x => x.CreatedAt >= from);
        }

        if (toDate.HasValue)
        {
            var to = toDate.Value.Date.AddDays(1).AddTicks(-1);
            ordersQuery = ordersQuery.Where(x => x.CreatedAt <= to);
        }

        var rows = await ordersQuery
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new SalesReportRowDto
            {
                InvoiceDate = x.CreatedAt,
                InvoiceNumber = x.Invoices.OrderByDescending(i => i.IssueDate).Select(i => i.InvoiceNumber).FirstOrDefault() ?? x.OrderNumber,
                CustomerNameEn = x.User.FullName ?? x.User.Email,
                CustomerNameAr = x.User.FullName ?? x.User.Email,
                TotalAmount = x.Total,
                DiscountAmount = x.DiscountAmount,
                NetAmount = x.Total - x.DiscountAmount,
                PaidAmount = x.Payments.Where(p => !p.IsRefunded).Sum(p => p.Amount),
                RemainingAmount = x.Total - x.Payments.Where(p => !p.IsRefunded).Sum(p => p.Amount)
            })
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }

    // ── Trial Balance ──────────────────────────────────────────────────────────

    /// <summary>
    /// GET /api/v1/Reports/trial-balance
    /// Aggregates JournalEntryLines grouped by Account.
    /// Supports optional date range or fiscal period filter.
    /// </summary>
    [HttpGet("trial-balance")]
    public async Task<ActionResult> GetTrialBalance(
        [FromQuery] Guid? fiscalPeriodId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var linesQuery = _context.Set<JournalEntryLine>()
            .Include(l => l.JournalEntry)
            .Include(l => l.Account).ThenInclude(a => a.AccountType)
            .Where(l => l.JournalEntry.IsPosted)
            .AsQueryable();

        if (fiscalPeriodId.HasValue)
            linesQuery = linesQuery.Where(l => l.JournalEntry.FiscalPeriodId == fiscalPeriodId.Value);
        if (from.HasValue)
            linesQuery = linesQuery.Where(l => l.JournalEntry.EntryDate >= from.Value);
        if (to.HasValue)
            linesQuery = linesQuery.Where(l => l.JournalEntry.EntryDate <= to.Value);

        var rows = await linesQuery
            .GroupBy(l => new
            {
                l.AccountId,
                l.Account.Code,
                l.Account.NameEn,
                l.Account.NameAr,
                AccountTypeName = l.Account.AccountType.NameEn,
                NormalBalance   = l.Account.AccountType.NormalBalance
            })
            .Select(g => new TrialBalanceRowDto
            {
                AccountId       = g.Key.AccountId,
                AccountCode     = g.Key.Code,
                AccountNameEn   = g.Key.NameEn,
                AccountNameAr   = g.Key.NameAr,
                AccountTypeName = g.Key.AccountTypeName,
                NormalBalance   = g.Key.NormalBalance,
                TotalDebit      = g.Sum(l => l.Debit),
                TotalCredit     = g.Sum(l => l.Credit)
            })
            .OrderBy(r => r.AccountCode)
            .ToListAsync(cancellationToken);

        // Net balance: debit-normal = debit - credit; credit-normal = credit - debit
        foreach (var row in rows)
        {
            row.NetBalance = row.NormalBalance == "Debit"
                ? row.TotalDebit - row.TotalCredit
                : row.TotalCredit - row.TotalDebit;
        }

        var summary = new
        {
            rows,
            totalDebit  = rows.Sum(r => r.TotalDebit),
            totalCredit = rows.Sum(r => r.TotalCredit),
            isBalanced  = rows.Sum(r => r.TotalDebit) == rows.Sum(r => r.TotalCredit)
        };

        return Ok(summary);
    }

    // ── General Ledger ────────────────────────────────────────────────────────

    /// <summary>
    /// GET /api/v1/Reports/ledger?accountId=...
    /// Chronological list of journal lines for one account with running balance.
    /// </summary>
    [HttpGet("ledger")]
    public async Task<ActionResult> GetLedger(
        [FromQuery] Guid accountId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? fiscalPeriodId,
        CancellationToken cancellationToken)
    {
        var account = await _context.Set<Account>()
            .Include(a => a.AccountType)
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
        if (account == null) return NotFound(new { error = "Account not found." });

        var linesQuery = _context.Set<JournalEntryLine>()
            .Include(l => l.JournalEntry)
            .Where(l => l.AccountId == accountId && l.JournalEntry.IsPosted)
            .AsQueryable();

        if (fiscalPeriodId.HasValue)
            linesQuery = linesQuery.Where(l => l.JournalEntry.FiscalPeriodId == fiscalPeriodId.Value);
        if (from.HasValue)
            linesQuery = linesQuery.Where(l => l.JournalEntry.EntryDate >= from.Value);
        if (to.HasValue)
            linesQuery = linesQuery.Where(l => l.JournalEntry.EntryDate <= to.Value);

        var lines = await linesQuery
            .OrderBy(l => l.JournalEntry.EntryDate)
            .ThenBy(l => l.JournalEntry.EntryNumber)
            .Select(l => new LedgerLineDto
            {
                EntryDate            = l.JournalEntry.EntryDate,
                EntryNumber          = l.JournalEntry.EntryNumber,
                Description          = l.Description ?? l.JournalEntry.Description,
                SourceDocumentType   = l.JournalEntry.SourceDocumentType,
                SourceDocumentNumber = l.JournalEntry.SourceDocumentNumber,
                Debit                = l.Debit,
                Credit               = l.Credit
            })
            .ToListAsync(cancellationToken);

        // Compute running balance
        bool isDebitNormal = account.AccountType.NormalBalance == "Debit";
        decimal running = 0;
        foreach (var line in lines)
        {
            running += isDebitNormal ? (line.Debit - line.Credit) : (line.Credit - line.Debit);
            line.RunningBalance = running;
        }

        return Ok(new
        {
            accountId   = account.Id,
            accountCode = account.Code,
            accountName = account.NameEn,
            normalBalance = account.AccountType.NormalBalance,
            lines,
            closingBalance = running
        });
    }

    // ── DTOs ─────────────────────────────────────────────────────────────────

    public class SalesReportRowDto
    {
        public DateTime InvoiceDate { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string? CustomerNameAr { get; set; }
        public string? CustomerNameEn { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
    }

    public class TrialBalanceRowDto
    {
        public Guid AccountId { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string AccountNameEn { get; set; } = string.Empty;
        public string AccountNameAr { get; set; } = string.Empty;
        public string AccountTypeName { get; set; } = string.Empty;
        public string NormalBalance { get; set; } = string.Empty;
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal NetBalance { get; set; }
    }

    public class LedgerLineDto
    {
        public DateTime EntryDate { get; set; }
        public string EntryNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? SourceDocumentType { get; set; }
        public string? SourceDocumentNumber { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal RunningBalance { get; set; }
    }
}
