using Maba.Domain.Common;

namespace Maba.Domain.Downloads;

/// <summary>
/// A downloadable file (datasheet, manual, certificate, etc.) attached to any entity.
/// Polymorphic via EntityType + EntityId — reusable for Items, Projects, Machines, etc.
/// </summary>
public class DownloadableFile : BaseEntity
{
    /// <summary>
    /// Type of the parent entity, e.g. "Item", "Project", "Machine", "Software".
    /// Used as the discriminator for polymorphic attachment.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Id of the parent entity record.
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>Admin-set display title (e.g. "Technical Datasheet Rev 2").</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional short description shown below the title.</summary>
    public string? Description { get; set; }

    /// <summary>Category key — matches DownloadCategory constants.</summary>
    public string Category { get; set; } = DownloadCategory.Other;

    /// <summary>Original file name as uploaded.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Relative storage path returned by IFileStorageService.SaveFileAsync.</summary>
    public string StoredPath { get; set; } = string.Empty;

    /// <summary>MIME content type (e.g. "application/pdf").</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>File size in bytes.</summary>
    public long FileSizeBytes { get; set; }

    /// <summary>Display order — lower numbers appear first.</summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>When false the file is hidden from public pages.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Marks one file as the primary/featured download.</summary>
    public bool IsFeatured { get; set; } = false;

    /// <summary>Download counter.</summary>
    public int DownloadCount { get; set; } = 0;

    /// <summary>UserId of the admin who uploaded the file.</summary>
    public string? UploadedBy { get; set; }
}

/// <summary>Well-known category constants — match the frontend dropdown.</summary>
public static class DownloadCategory
{
    public const string Datasheet          = "Datasheet";
    public const string UserManual         = "UserManual";
    public const string TechnicalDocument  = "TechnicalDocument";
    public const string Brochure           = "Brochure";
    public const string Certificate        = "Certificate";
    public const string InstallationGuide  = "InstallationGuide";
    public const string Firmware           = "Firmware";
    public const string Download           = "Download";
    public const string Other              = "Other";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Datasheet, UserManual, TechnicalDocument, Brochure,
        Certificate, InstallationGuide, Firmware, Download, Other
    };
}
