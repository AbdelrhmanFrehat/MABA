using Maba.Domain.Cnc;

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

    public string? PcbMaterialType { get; set; }
    public string? SupportedBoardThicknesses { get; set; }
    public bool SupportsSingleSided { get; set; }
    public bool SupportsDoubleSided { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static CncMaterialDto FromEntity(CncMaterial m) => new()
    {
        Id = m.Id,
        NameEn = m.NameEn,
        NameAr = m.NameAr,
        DescriptionEn = m.DescriptionEn,
        DescriptionAr = m.DescriptionAr,
        Type = m.Type,
        MinThicknessMm = m.MinThicknessMm,
        MaxThicknessMm = m.MaxThicknessMm,
        IsMetal = m.IsMetal,
        IsActive = m.IsActive,
        SortOrder = m.SortOrder,
        AllowCut = m.AllowCut,
        AllowEngrave = m.AllowEngrave,
        AllowPocket = m.AllowPocket,
        AllowDrill = m.AllowDrill,
        MaxCutDepthMm = m.MaxCutDepthMm,
        MaxEngraveDepthMm = m.MaxEngraveDepthMm,
        MaxPocketDepthMm = m.MaxPocketDepthMm,
        MaxDrillDepthMm = m.MaxDrillDepthMm,
        CutNotesEn = m.CutNotesEn,
        CutNotesAr = m.CutNotesAr,
        EngraveNotesEn = m.EngraveNotesEn,
        EngraveNotesAr = m.EngraveNotesAr,
        PocketNotesEn = m.PocketNotesEn,
        PocketNotesAr = m.PocketNotesAr,
        DrillNotesEn = m.DrillNotesEn,
        DrillNotesAr = m.DrillNotesAr,
        NotesEn = m.NotesEn,
        NotesAr = m.NotesAr,
        IsPcbOnly = m.IsPcbOnly,
        PcbMaterialType = m.PcbMaterialType,
        SupportedBoardThicknesses = m.SupportedBoardThicknesses,
        SupportsSingleSided = m.SupportsSingleSided,
        SupportsDoubleSided = m.SupportsDoubleSided,
        CreatedAt = m.CreatedAt,
        UpdatedAt = m.UpdatedAt
    };
}
