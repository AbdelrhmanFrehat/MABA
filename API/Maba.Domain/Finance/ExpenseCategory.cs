using Maba.Domain.Common;

namespace Maba.Domain.Finance;

public class ExpenseCategory : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}

