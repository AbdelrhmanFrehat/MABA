namespace Maba.Application.Features.Media.DTOs;

public class MediaAssetDto
{
    public Guid Id { get; set; }
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
    public Guid? UploadedByUserId { get; set; }
    public Guid MediaTypeId { get; set; }
    public string MediaTypeKey { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

