namespace Maba.Application.Features.Printing.FilamentSpools;

public class FilamentSpoolDto
{
    public Guid Id { get; set; }
    public Guid MaterialId { get; set; }
    public string MaterialNameEn { get; set; } = string.Empty;
    public string? MaterialNameAr { get; set; }
    public Guid? MaterialColorId { get; set; }
    public string? ColorNameEn { get; set; }
    public string? ColorNameAr { get; set; }
    public string? ColorHexCode { get; set; }
    public string? Name { get; set; }
    public int InitialWeightGrams { get; set; }
    public int RemainingWeightGrams { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateFilamentSpoolDto
{
    public Guid MaterialId { get; set; }
    public Guid MaterialColorId { get; set; }
    public string? Name { get; set; }
    public int InitialWeightGrams { get; set; } = 1000;
}

public class UpdateFilamentSpoolDto
{
    public string? Name { get; set; }
    public int RemainingWeightGrams { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Creates a spool and, if MaterialColorId is null, creates a new color inline from NewColor* fields.
/// </summary>
public class CreateSpoolWithColorDto
{
    public Guid MaterialId { get; set; }

    // Option A: use existing color
    public Guid? MaterialColorId { get; set; }

    // Option B: create new color inline
    public string? NewColorNameEn { get; set; }
    public string? NewColorNameAr { get; set; }
    public string? NewColorHexCode { get; set; }

    public string? Name { get; set; }
    public int InitialWeightGrams { get; set; } = 1000;
}
