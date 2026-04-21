using Maba.Domain.Common;
using Maba.Domain.MachineCatalog.Sections;

namespace Maba.Domain.MachineCatalog;

/// <summary>
/// Official machine definition — the source of truth for the entire MABA ecosystem.
/// Identity fields are stored as direct columns; complex sections are stored as JSON columns.
/// RuntimeProfile is NOT stored here — it is app-local only.
/// </summary>
public class MachineDefinition : BaseEntity
{
    // ── Identity ──────────────────────────────────────────────────────────

    public string Code { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public string? RevisionNote { get; set; }

    public Guid CategoryId { get; set; }
    public MachineCategory Category { get; set; } = null!;

    public Guid FamilyId { get; set; }
    public MachineFamily Family { get; set; } = null!;

    public string DisplayNameEn { get; set; } = string.Empty;
    public string DisplayNameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string Manufacturer { get; set; } = "MABA";

    /// <summary>JSON-serialized string array stored as nvarchar(max).</summary>
    public string TagsJson { get; set; } = "[]";

    public bool IsActive { get; set; } = true;
    public bool IsPublic { get; set; } = true;
    public bool IsDeprecated { get; set; } = false;
    public string? DeprecationNote { get; set; }
    public int SortOrder { get; set; }
    public DateTime? ReleasedAt { get; set; }

    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }

    /// <summary>Never exposed in app-facing API responses.</summary>
    public string? InternalNotes { get; set; }

    // ── Sections (stored as JSON columns) ─────────────────────────────────

    public RuntimeBindingSection RuntimeBinding { get; set; } = new();
    public AxisConfigSection AxisConfig { get; set; } = new();
    public WorkspaceSection Workspace { get; set; } = new();
    public MotionDefaultsSection MotionDefaults { get; set; } = new();
    public ConnectionDefaultsSection ConnectionDefaults { get; set; } = new();
    public CapabilitiesSection Capabilities { get; set; } = new();
    public FileSupportSection FileSupport { get; set; } = new();
    public VisualizationSection Visualization { get; set; } = new();
    public ProfileRulesSection ProfileRules { get; set; } = new();
}
