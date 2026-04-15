namespace Maba.Application.Features.Assets.DTOs;

public class AssetDto
{
    public Guid Id { get; set; }
    public string AssetNumber { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }

    public Guid AssetCategoryId { get; set; }
    public string? AssetCategoryNameEn { get; set; }
    public string? AssetCategoryNameAr { get; set; }

    public Guid InvestorUserId { get; set; }
    public string? InvestorUserFullName { get; set; }

    public Guid AcquisitionConditionId { get; set; }
    public string? AcquisitionConditionKey { get; set; }
    public string? AcquisitionConditionNameEn { get; set; }
    public string? AcquisitionConditionNameAr { get; set; }
    public string? ConditionNotes { get; set; }

    public decimal ApproximatePrice { get; set; }
    public string Currency { get; set; } = "ILS";
    public DateTime AcquiredAt { get; set; }

    public Guid StatusId { get; set; }
    public string? StatusKey { get; set; }
    public string? StatusNameEn { get; set; }
    public string? StatusNameAr { get; set; }

    public string? LocationNotes { get; set; }
    public Guid? PhotoMediaId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class AssetCategoryDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? NumberingPrefix { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class AssetNumberingSettingDto
{
    public Guid Id { get; set; }
    public string Prefix { get; set; } = "A-";
    public int PadWidth { get; set; } = 4;
    public long NextNumber { get; set; } = 1;
}
