namespace Maba.Application.Features.Printing.DTOs;

public class MaterialDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public decimal PricePerGram { get; set; }
    public decimal Density { get; set; }
    public string? Color { get; set; }
    public bool IsActive { get; set; }
    public decimal StockQuantity { get; set; }
    public List<MaterialColorDto> AvailableColors { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class MaterialColorDto
{
    public Guid Id { get; set; }
    public Guid MaterialId { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string HexCode { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateMaterialColorDto
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string HexCode { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateMaterialColorDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string HexCode { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}

public class PrinterDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Vendor { get; set; }
    public decimal BuildVolumeX { get; set; }
    public decimal BuildVolumeY { get; set; }
    public decimal BuildVolumeZ { get; set; }
    public Guid PrintingTechnologyId { get; set; }
    public string PrintingTechnologyKey { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? CurrentStatus { get; set; }
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SlicingProfileDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public Guid PrintingTechnologyId { get; set; }
    public decimal LayerHeightMm { get; set; }
    public decimal InfillPercent { get; set; }
    public bool SupportsEnabled { get; set; }
    public Guid MaterialId { get; set; }
    public Guid? PrinterId { get; set; }
    public bool IsDefault { get; set; }
    public string? TemperatureSettings { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class DesignDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsPublic { get; set; }
    public string? Tags { get; set; }
    public string? LicenseType { get; set; }
    public int DownloadCount { get; set; }
    public int LikeCount { get; set; }
    public List<DesignFileDto> Files { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class DesignDetailDto : DesignDto
{
    public int SlicingJobsCount { get; set; }
    public int PrintJobsCount { get; set; }
}

public class DesignFileDto
{
    public Guid Id { get; set; }
    public Guid DesignId { get; set; }
    public Guid MediaAssetId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SlicingJobDto
{
    public Guid Id { get; set; }
    public Guid DesignFileId { get; set; }
    public Guid SlicingProfileId { get; set; }
    public Guid SlicingJobStatusId { get; set; }
    public string SlicingJobStatusKey { get; set; } = string.Empty;
    public int? EstimatedTimeMin { get; set; }
    public decimal? EstimatedMaterialGrams { get; set; }
    public decimal? PriceEstimate { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string? ErrorMessage { get; set; }
    public string? OutputFileUrl { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SlicingJobDetailDto : SlicingJobDto
{
    public DesignFileDto? DesignFile { get; set; }
    public SlicingProfileDto? SlicingProfile { get; set; }
    public int PrintJobsCount { get; set; }
}

public class PrintJobDto
{
    public Guid Id { get; set; }
    public Guid SlicingJobId { get; set; }
    public Guid PrinterId { get; set; }
    public string PrinterNameEn { get; set; } = string.Empty;
    public Guid PrintJobStatusId { get; set; }
    public string PrintJobStatusKey { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public decimal? ActualMaterialGrams { get; set; }
    public int? ActualTimeMin { get; set; }
    public decimal? FinalPrice { get; set; }
    public int ProgressPercent { get; set; }
    public DateTime? EstimatedCompletionTime { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PrintJobDetailDto : PrintJobDto
{
    public SlicingJobDto? SlicingJob { get; set; }
    public PrinterDto? Printer { get; set; }
}

public class PrintQualityProfileDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public decimal LayerHeightMm { get; set; }
    public string SpeedCategory { get; set; } = "Normal";
    public decimal PriceMultiplier { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}