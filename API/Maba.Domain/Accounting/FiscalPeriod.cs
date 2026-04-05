using Maba.Domain.Common;

namespace Maba.Domain.Accounting;

public class FiscalPeriod : BaseEntity
{
    public Guid FiscalYearId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int PeriodNumber { get; set; }
    public bool IsClosed { get; set; }

    public FiscalYear FiscalYear { get; set; } = null!;
    public ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
}
