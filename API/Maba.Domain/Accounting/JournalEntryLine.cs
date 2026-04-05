using Maba.Domain.Common;

namespace Maba.Domain.Accounting;

public class JournalEntryLine : BaseEntity
{
    public Guid JournalEntryId { get; set; }
    public Guid AccountId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Description { get; set; }
    public decimal? CurrencyAmount { get; set; }
    public decimal? ExchangeRate { get; set; }
    public int SortOrder { get; set; }

    public JournalEntry JournalEntry { get; set; } = null!;
    public Account Account { get; set; } = null!;
}
