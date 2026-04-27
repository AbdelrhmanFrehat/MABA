using Maba.Domain.Common;

namespace Maba.Domain.Storage;

public class StorageVariant : BaseEntity
{
    public Guid ParentId { get; set; }

    /// <summary>Display label, e.g. "10kΩ 0805 ±5%". Auto-generated if left blank.</summary>
    public string? VariantLabel { get; set; }

    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; } = 0;
    public string Unit { get; set; } = "pcs";

    // Component specs
    public string? Package { get; set; }       // 0805, TO-220, DIP-8
    public string? Value { get; set; }          // 10, 100, 47
    public string? ValueUnit { get; set; }      // kΩ, nF, µH
    public string? Tolerance { get; set; }      // ±5%, ±1%
    public string? VoltageRating { get; set; }  // 50V
    public string? CurrentRating { get; set; }  // 1A
    public string? PowerRating { get; set; }    // 0.25W

    public string? Notes { get; set; }
    public string? ImageUrl { get; set; }
    public string? DatasheetUrl { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsPublishedToShop { get; set; } = false;

    public StorageParent Parent { get; set; } = null!;
}
