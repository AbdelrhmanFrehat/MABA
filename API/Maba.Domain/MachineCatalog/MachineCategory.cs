using Maba.Domain.Common;

namespace Maba.Domain.MachineCatalog;

/// <summary>
/// High-level machine type (CNC, Printer3D, Laser, Robotics, Generic).
/// Backend-owned, app-facing read-only.
/// </summary>
public class MachineCategory : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string DisplayNameEn { get; set; } = string.Empty;
    public string DisplayNameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? IconKey { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<MachineFamily> Families { get; set; } = new List<MachineFamily>();
}
