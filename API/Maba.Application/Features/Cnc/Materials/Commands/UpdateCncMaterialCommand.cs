using MediatR;
using Maba.Application.Features.Cnc.DTOs;

namespace Maba.Application.Features.Cnc.Materials.Commands;

public class UpdateCncMaterialCommand : IRequest<CncMaterialDto?>
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? Type { get; set; } = "routing";
    
    // Core Constraints
    public decimal? MinThicknessMm { get; set; }
    public decimal? MaxThicknessMm { get; set; }
    public bool IsMetal { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    
    // Operation Flags
    public bool AllowCut { get; set; } = true;
    public bool AllowEngrave { get; set; } = true;
    public bool AllowPocket { get; set; } = true;
    public bool AllowDrill { get; set; } = true;
    
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
}
