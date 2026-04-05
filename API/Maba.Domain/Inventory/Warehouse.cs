using Maba.Domain.Common;

namespace Maba.Domain.Inventory;

public class Warehouse : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? ManagerUserId { get; set; }
}
