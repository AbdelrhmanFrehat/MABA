using Maba.Domain.Common;

namespace Maba.Domain.Catalog;

public class ItemStatus : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<Item> Items { get; set; } = new List<Item>();
}

