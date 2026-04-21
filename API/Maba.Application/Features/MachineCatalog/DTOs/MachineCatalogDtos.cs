using Maba.Domain.MachineCatalog.Sections;

namespace Maba.Application.Features.MachineCatalog.DTOs;

// ── MachineCategory ───────────────────────────────────────────────────────

public class MachineCategoryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DisplayNameEn { get; set; } = string.Empty;
    public string DisplayNameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? IconKey { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// ── MachineFamily ─────────────────────────────────────────────────────────

public class MachineFamilyDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryDisplayNameEn { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DisplayNameEn { get; set; } = string.Empty;
    public string DisplayNameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string Manufacturer { get; set; } = "MABA";
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// ── MachineDefinitionSummary (list endpoint) ──────────────────────────────

public class MachineDefinitionSummaryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string? CategoryDisplayNameEn { get; set; }
    public Guid FamilyId { get; set; }
    public string? FamilyDisplayNameEn { get; set; }
    public string DisplayNameEn { get; set; } = string.Empty;
    public string DisplayNameAr { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public bool IsDeprecated { get; set; }
    public string? DeprecationNote { get; set; }
    public int SortOrder { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public List<string> Tags { get; set; } = new();
    public string DefaultDriverType { get; set; } = string.Empty;
    public List<string> SupportedSetupModes { get; set; } = new();
    public string RuntimeUiVariant { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// ── MachineDefinition full (detail endpoint) ──────────────────────────────

public class MachineDefinitionDto
{
    // Identity
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? RevisionNote { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryDisplayNameEn { get; set; }
    public Guid FamilyId { get; set; }
    public string? FamilyDisplayNameEn { get; set; }
    public string DisplayNameEn { get; set; } = string.Empty;
    public string DisplayNameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public bool IsDeprecated { get; set; }
    public string? DeprecationNote { get; set; }
    public int SortOrder { get; set; }
    public DateTime? ReleasedAt { get; set; }
    // internalNotes intentionally omitted — populated only by admin-specific endpoint
    public string? InternalNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Sections
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

// ── Request models ────────────────────────────────────────────────────────

public class CreateMachineCategoryRequest
{
    public string Code { get; set; } = string.Empty;
    public string DisplayNameEn { get; set; } = string.Empty;
    public string DisplayNameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? IconKey { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateMachineCategoryRequest : CreateMachineCategoryRequest
{
    public Guid Id { get; set; }
}

public class CreateMachineFamilyRequest
{
    public Guid CategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DisplayNameEn { get; set; } = string.Empty;
    public string DisplayNameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string Manufacturer { get; set; } = "MABA";
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

public class UpdateMachineFamilyRequest : CreateMachineFamilyRequest
{
    public Guid Id { get; set; }
}

public class CreateMachineDefinitionRequest
{
    public string Code { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public string? RevisionNote { get; set; }
    public Guid CategoryId { get; set; }
    public Guid FamilyId { get; set; }
    public string DisplayNameEn { get; set; } = string.Empty;
    public string DisplayNameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string Manufacturer { get; set; } = "MABA";
    public List<string> Tags { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public bool IsPublic { get; set; } = true;
    public bool IsDeprecated { get; set; } = false;
    public string? DeprecationNote { get; set; }
    public int SortOrder { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public string? InternalNotes { get; set; }

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

public class UpdateMachineDefinitionRequest : CreateMachineDefinitionRequest
{
    public Guid Id { get; set; }
}

public class PatchMachineDefinitionStatusRequest
{
    public bool? IsActive { get; set; }
    public bool? IsDeprecated { get; set; }
    public string? DeprecationNote { get; set; }
    public bool? IsPublic { get; set; }
}

public class MachineDefinitionListQuery
{
    public Guid? CategoryId { get; set; }
    public Guid? FamilyId { get; set; }
    public bool ActiveOnly { get; set; } = true;
    public bool IncludeDeprecated { get; set; } = false;
    public bool AdminMode { get; set; } = false;
    public string? Search { get; set; }
}
