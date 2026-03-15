using Maba.Domain.Common;

namespace Maba.Domain.Laser;

public class LaserMaterial : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of laser operation: "cut", "engrave", or "both"
    /// </summary>
    public string Type { get; set; } = "both";
    
    public decimal? MinThicknessMm { get; set; }
    public decimal? MaxThicknessMm { get; set; }
    
    public string? NotesEn { get; set; }
    public string? NotesAr { get; set; }
    
    /// <summary>
    /// If true, this is a metal material (cutting not supported)
    /// </summary>
    public bool IsMetal { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}
