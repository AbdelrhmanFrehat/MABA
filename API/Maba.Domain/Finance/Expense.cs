using Maba.Domain.Common;
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
    
    // Navigation properties
    public ExpenseCategory ExpenseCategory { get; set; } = null!;
    public MediaAsset? ReceiptMedia { get; set; }
    public User EnteredByUser { get; set; } = null!;
}

