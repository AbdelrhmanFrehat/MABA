using Maba.Domain.Common;

namespace Maba.Domain.Media;

public class MediaAsset : BaseEntity
{
    public string FileUrl { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? TitleEn { get; set; }
    public string? TitleAr { get; set; }
    public string? AltEn { get; set; }
    public string? AltAr { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? AltTextEn { get; set; }
    public string? AltTextAr { get; set; }
    public string StorageProvider { get; set; } = "Local"; // Local, S3, AzureBlob, etc.
    public string? StorageKey { get; set; } // Storage path/key for cloud providers
    public bool IsPublic { get; set; } = true;
    public Guid? UploadedByUserId { get; set; }
    public Guid MediaTypeId { get; set; }
    
    // Navigation properties
    public MediaType MediaType { get; set; } = null!;
    public ICollection<EntityMediaLink> EntityMediaLinks { get; set; } = new List<EntityMediaLink>();
}

