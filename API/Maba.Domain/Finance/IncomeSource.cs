using Maba.Domain.Common;

namespace Maba.Domain.Finance;

public class IncomeSource : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<Income> Incomes { get; set; } = new List<Income>();
}

