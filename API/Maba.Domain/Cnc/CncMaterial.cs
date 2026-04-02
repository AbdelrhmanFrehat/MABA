using Maba.Domain.Common;

namespace Maba.Domain.Cnc;

public class CncMaterial : BaseEntity
{
    // Basic Info
    public string NameEn { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? Type { get; set; }
    
    // Core Constraints
    public decimal? MinThicknessMm { get; set; }
    public decimal? MaxThicknessMm { get; set; }
    public bool IsMetal { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    
    // Operation Flags (Routing mode only)
    public bool AllowCut { get; set; } = true;
    public bool AllowEngrave { get; set; } = true;
    public bool AllowPocket { get; set; } = true;
    public bool AllowDrill { get; set; } = true;
    
    // Depth Constraints (null = up to thickness)
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
    
    // PCB-only flag (material only available in PCB mode; false when Type is "both")
    public bool IsPcbOnly { get; set; }

    /// <summary>PCB substrate family (e.g. FR4, FR1). Used when Type is pcb or both.</summary>
    public string? PcbMaterialType { get; set; }

    /// <summary>Comma-separated board thicknesses in mm (e.g. "0.8,1.0,1.6").</summary>
    public string? SupportedBoardThicknesses { get; set; }

    public bool SupportsSingleSided { get; set; } = true;
    public bool SupportsDoubleSided { get; set; } = true;
}
