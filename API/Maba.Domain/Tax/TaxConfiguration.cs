using Maba.Domain.Accounting;
using Maba.Domain.Common;

namespace Maba.Domain.Tax;

public class TaxConfiguration : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? AccountId { get; set; }

    public Account? Account { get; set; }
}
