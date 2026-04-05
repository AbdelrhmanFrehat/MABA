using Maba.Domain.Common;

namespace Maba.Domain.Lookups;

public class LookupType : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<LookupValue> Values { get; set; } = new List<LookupValue>();
}
