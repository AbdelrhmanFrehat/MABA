namespace Maba.Application.Features.Cnc.DTOs;

public class CncMaterialDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? Type { get; set; }
    
    // Core Constraints
    public decimal? MinThicknessMm { get; set; }
    public decimal? MaxThicknessMm { get; set; }
    public bool IsMetal { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    
    // Operation Flags
    public bool AllowCut { get; set; }
    public bool AllowEngrave { get; set; }
    public bool AllowPocket { get; set; }
    public bool AllowDrill { get; set; }
    
    // Depth Constraints
    public decimal? MaxCutDepthMm { get; set; }
    public decimal? MaxEngraveDepthMm { get; set; }
    public decimal? MaxPocketDepthMm { get; set; }
    public decimal? MaxDrillDepthMm { get; set; }
    
    // Operation-specific Notes
    public string? CutNotesEn { get; set; }
    public string? CutNotesAr { get; set; }
    public string? EngraveNotesEn { get; set; }
    public string? EngraveNotesAr { get; set; }
    public string? PocketNotesEn { get; set; }
    public string? PocketNotesAr { get; set; }
    public string? DrillNotesEn { get; set; }
    public string? DrillNotesAr { get; set; }
    
    // General Notes
    public string? NotesEn { get; set; }
    public string? NotesAr { get; set; }
    
    // PCB-only flag
    public bool IsPcbOnly { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
