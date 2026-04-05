using Maba.Domain.Common;
using Maba.Domain.Lookups;

namespace Maba.Domain.Accounting;

public class JournalEntry : BaseEntity
{
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public Guid? FiscalPeriodId { get; set; }
    public Guid JournalEntryTypeId { get; set; }
    public string? Description { get; set; }
    public string? SourceDocumentType { get; set; }
    public Guid? SourceDocumentId { get; set; }
    public string? SourceDocumentNumber { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public bool IsPosted { get; set; }
    public DateTime? PostedAt { get; set; }
    public Guid? PostedByUserId { get; set; }
    public bool IsReversed { get; set; }
    public Guid? ReversedByEntryId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? Notes { get; set; }

    public FiscalPeriod? FiscalPeriod { get; set; }
    public LookupValue JournalEntryType { get; set; } = null!;
    public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
}
