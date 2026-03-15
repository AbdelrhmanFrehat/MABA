namespace Maba.Domain.Catalog;

public class ItemTag
{
    public Guid ItemId { get; set; }
    public Guid TagId { get; set; }
    
    // Navigation properties
    public Item Item { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}

