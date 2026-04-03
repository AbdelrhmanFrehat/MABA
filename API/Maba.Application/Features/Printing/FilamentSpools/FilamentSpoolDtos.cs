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
    public string? Name { get; set; }
    public int InitialWeightGrams { get; set; }
    public int RemainingWeightGrams { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateFilamentSpoolDto
{
    public Guid MaterialId { get; set; }
    public Guid? MaterialColorId { get; set; }
    public string? Name { get; set; }
    public int InitialWeightGrams { get; set; } = 1000;
}

public class UpdateFilamentSpoolDto
{
    public string? Name { get; set; }
    public int RemainingWeightGrams { get; set; }
    public bool IsActive { get; set; } = true;
}
