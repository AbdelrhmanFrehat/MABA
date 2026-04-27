using Maba.Domain.Common;

namespace Maba.Domain.Storage;

public class StorageParent : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = "Other";
    public string? Manufacturer { get; set; }
    public string? ImageUrl { get; set; }
    public string? DatasheetUrl { get; set; }
    public bool IsPublishedToShop { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;

    public ICollection<StorageVariant> Variants { get; set; } = new List<StorageVariant>();
}
