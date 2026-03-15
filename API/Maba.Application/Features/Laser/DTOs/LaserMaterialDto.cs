namespace Maba.Application.Features.Laser.DTOs;

public class LaserMaterialDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Type { get; set; } = "both";
    public decimal? MinThicknessMm { get; set; }
    public decimal? MaxThicknessMm { get; set; }
    public string? NotesEn { get; set; }
    public string? NotesAr { get; set; }
    public bool IsMetal { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
