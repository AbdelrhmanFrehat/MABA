using Maba.Domain.Common;
using Maba.Domain.Catalog;

namespace Maba.Domain.Machines;

public class ItemMachineLink : BaseEntity
{
    public Guid ItemId { get; set; }
    public Guid MachineId { get; set; }
    public Guid? MachinePartId { get; set; }
    
    // Navigation properties
    public Item Item { get; set; } = null!;
    public Machine Machine { get; set; } = null!;
    public MachinePart? MachinePart { get; set; }
}

