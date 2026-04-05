using Maba.Domain.Common;

namespace Maba.Domain.Accounting;

public class AccountType : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NormalBalance { get; set; } = string.Empty;

    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}
