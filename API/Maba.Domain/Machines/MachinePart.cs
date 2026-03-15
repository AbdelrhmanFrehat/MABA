using Maba.Domain.Common;

namespace Maba.Domain.Machines;

public class MachinePart : BaseEntity
{
    public Guid MachineId { get; set; }
    public string PartNameEn { get; set; } = string.Empty;
    public string PartNameAr { get; set; } = string.Empty;
    public string? PartCode { get; set; }
    public decimal? Price { get; set; }
    public Guid? InventoryId { get; set; } // Link to Inventory if this part is sold as an item
    public Guid? ImageId { get; set; } // MediaAsset reference
    
    // Navigation properties
    public Machine Machine { get; set; } = null!;
    public ICollection<ItemMachineLink> ItemMachineLinks { get; set; } = new List<ItemMachineLink>();
}

