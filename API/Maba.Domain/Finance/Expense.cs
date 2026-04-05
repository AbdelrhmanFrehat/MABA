using Maba.Domain.Common;
using Maba.Domain.Accounting;
using Maba.Domain.Crm;
using Maba.Domain.Lookups;
using Maba.Domain.Media;
using Maba.Domain.Users;

namespace Maba.Domain.Finance;

public class Expense : BaseEntity
{
    public Guid ExpenseCategoryId { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime SpentAt { get; set; }
    public Guid? ReceiptMediaId { get; set; }
    public Guid EnteredByUserId { get; set; }
    public Guid? ApprovalStatusId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public bool IsPosted { get; set; }
    public Guid? JournalEntryId { get; set; }
    public Guid? AccountId { get; set; }
    public Guid? SupplierId { get; set; }
    
    // Navigation properties
    public ExpenseCategory ExpenseCategory { get; set; } = null!;
    public MediaAsset? ReceiptMedia { get; set; }
    public User EnteredByUser { get; set; } = null!;
    public LookupValue? ApprovalStatus { get; set; }
    public User? ApprovedByUser { get; set; }
    public JournalEntry? JournalEntry { get; set; }
    public Account? Account { get; set; }
    public Supplier? Supplier { get; set; }
}

