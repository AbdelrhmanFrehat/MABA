using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Accounting;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/journal-entries")]
[Authorize]
public class JournalEntriesController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public JournalEntriesController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> GetJournalEntries(
        [FromQuery] string? sourceDocumentType,
        [FromQuery] Guid? sourceDocumentId,
        [FromQuery] Guid? fiscalPeriodId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<JournalEntry>()
            .Include(x => x.Lines).ThenInclude(l => l.Account)
            .Include(x => x.FiscalPeriod)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(sourceDocumentType))
            query = query.Where(x => x.SourceDocumentType == sourceDocumentType);
        if (sourceDocumentId.HasValue)
            query = query.Where(x => x.SourceDocumentId == sourceDocumentId.Value);
        if (fiscalPeriodId.HasValue)
            query = query.Where(x => x.FiscalPeriodId == fiscalPeriodId.Value);
        if (from.HasValue)
            query = query.Where(x => x.EntryDate >= from.Value);
        if (to.HasValue)
            query = query.Where(x => x.EntryDate <= to.Value);

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.EntryDate)
            .ThenByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new JournalEntryDto
            {
                Id                   = x.Id,
                EntryNumber          = x.EntryNumber,
                EntryDate            = x.EntryDate,
                FiscalPeriodId       = x.FiscalPeriodId,
                FiscalPeriodName     = x.FiscalPeriod != null ? x.FiscalPeriod.Name : null,
                Description          = x.Description,
                SourceDocumentType   = x.SourceDocumentType,
                SourceDocumentId     = x.SourceDocumentId,
                SourceDocumentNumber = x.SourceDocumentNumber,
                TotalDebit           = x.TotalDebit,
                TotalCredit          = x.TotalCredit,
                IsPosted             = x.IsPosted,
                PostedAt             = x.PostedAt,
                IsReversed           = x.IsReversed,
                Notes                = x.Notes,
                Lines                = x.Lines.OrderBy(l => l.SortOrder).Select(l => new JournalEntryLineDto
                {
                    Id          = l.Id,
                    AccountId   = l.AccountId,
                    AccountCode = l.Account.Code,
                    AccountName = l.Account.NameEn,
                    Debit       = l.Debit,
                    Credit      = l.Credit,
                    Description = l.Description,
                    SortOrder   = l.SortOrder
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            items,
            totalCount = total,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(total / (double)pageSize)
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JournalEntryDto>> GetJournalEntry(Guid id, CancellationToken cancellationToken)
    {
        var x = await _context.Set<JournalEntry>()
            .Include(e => e.Lines).ThenInclude(l => l.Account)
            .Include(e => e.FiscalPeriod)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (x == null) return NotFound();

        return Ok(new JournalEntryDto
        {
            Id                   = x.Id,
            EntryNumber          = x.EntryNumber,
            EntryDate            = x.EntryDate,
            FiscalPeriodId       = x.FiscalPeriodId,
            FiscalPeriodName     = x.FiscalPeriod?.Name,
            Description          = x.Description,
            SourceDocumentType   = x.SourceDocumentType,
            SourceDocumentId     = x.SourceDocumentId,
            SourceDocumentNumber = x.SourceDocumentNumber,
            TotalDebit           = x.TotalDebit,
            TotalCredit          = x.TotalCredit,
            IsPosted             = x.IsPosted,
            PostedAt             = x.PostedAt,
            IsReversed           = x.IsReversed,
            Notes                = x.Notes,
            Lines                = x.Lines.OrderBy(l => l.SortOrder).Select(l => new JournalEntryLineDto
            {
                Id          = l.Id,
                AccountId   = l.AccountId,
                AccountCode = l.Account.Code,
                AccountName = l.Account.NameEn,
                Debit       = l.Debit,
                Credit      = l.Credit,
                Description = l.Description,
                SortOrder   = l.SortOrder
            }).ToList()
        });
    }

    // ── DTOs ──────────────────────────────────────────────────────────────────

    public class JournalEntryDto
    {
        public Guid Id { get; set; }
        public string EntryNumber { get; set; } = string.Empty;
        public DateTime EntryDate { get; set; }
        public Guid? FiscalPeriodId { get; set; }
        public string? FiscalPeriodName { get; set; }
        public string? Description { get; set; }
        public string? SourceDocumentType { get; set; }
        public Guid? SourceDocumentId { get; set; }
        public string? SourceDocumentNumber { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public bool IsPosted { get; set; }
        public DateTime? PostedAt { get; set; }
        public bool IsReversed { get; set; }
        public string? Notes { get; set; }
        public List<JournalEntryLineDto> Lines { get; set; } = new();
    }

    public class JournalEntryLineDto
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
    }
}
