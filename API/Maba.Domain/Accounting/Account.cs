using Maba.Domain.Common;

namespace Maba.Domain.Accounting;

public class Account : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public Guid AccountTypeId { get; set; }
    public Guid? ParentId { get; set; }
    public int Level { get; set; }
    public bool IsPostable { get; set; } = true;
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
    public string Currency { get; set; } = "ILS";
    public decimal CurrentBalance { get; set; }
    public string? Description { get; set; }

    public AccountType AccountType { get; set; } = null!;
    public Account? Parent { get; set; }
    public ICollection<Account> Children { get; set; } = new List<Account>();
    public ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();
}
