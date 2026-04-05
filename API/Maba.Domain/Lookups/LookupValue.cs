using Maba.Domain.Common;

namespace Maba.Domain.Lookups;

public class LookupValue : BaseEntity
{
    public Guid LookupTypeId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsDefault { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public Guid? ParentId { get; set; }
    public string? MetaJson { get; set; }

    public LookupType LookupType { get; set; } = null!;
    public LookupValue? Parent { get; set; }
    public ICollection<LookupValue> Children { get; set; } = new List<LookupValue>();
}
