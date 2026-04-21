using Maba.Domain.Common;

namespace Maba.Domain.MachineCatalog;

/// <summary>
/// A product/platform family within a category.
/// Backend-owned, app-facing read-only.
/// </summary>
public class MachineFamily : BaseEntity
{
    public Guid CategoryId { get; set; }
    public MachineCategory Category { get; set; } = null!;

    public string Code { get; set; } = string.Empty;
    public string DisplayNameEn { get; set; } = string.Empty;
    public string DisplayNameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string Manufacturer { get; set; } = "MABA";
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public ICollection<MachineDefinition> Definitions { get; set; } = new List<MachineDefinition>();
}
