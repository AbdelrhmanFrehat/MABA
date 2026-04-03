using Maba.Domain.Common;

namespace Maba.Domain.Printing;

/// <summary>
/// A physical filament spool tracked for inventory (Phase 1: CRUD only; not linked to print requests).
/// </summary>
public class FilamentSpool : BaseEntity
{
    public Guid MaterialId { get; set; }
    public Material Material { get; set; } = null!;

    public Guid? MaterialColorId { get; set; }
    public MaterialColor? MaterialColor { get; set; }

    /// <summary>Optional label, e.g. "PLA Black #1"</summary>
    public string? Name { get; set; }

    public int InitialWeightGrams { get; set; } = 1000;
    public int RemainingWeightGrams { get; set; } = 1000;

    public bool IsActive { get; set; } = true;
}
