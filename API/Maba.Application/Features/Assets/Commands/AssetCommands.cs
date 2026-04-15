using MediatR;
using Maba.Application.Features.Assets.DTOs;

namespace Maba.Application.Features.Assets.Commands;

public class CreateAssetCommand : IRequest<AssetDto>
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public Guid AssetCategoryId { get; set; }
    public Guid InvestorUserId { get; set; }
    public Guid AcquisitionConditionId { get; set; }
    public string? ConditionNotes { get; set; }
    public decimal ApproximatePrice { get; set; }
    public string Currency { get; set; } = "ILS";
    public DateTime AcquiredAt { get; set; }
    public Guid StatusId { get; set; }
    public string? LocationNotes { get; set; }
    public Guid? PhotoMediaId { get; set; }
}

public class UpdateAssetCommand : IRequest<AssetDto>
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public Guid AssetCategoryId { get; set; }
    public Guid InvestorUserId { get; set; }
    public Guid AcquisitionConditionId { get; set; }
    public string? ConditionNotes { get; set; }
    public decimal ApproximatePrice { get; set; }
    public string Currency { get; set; } = "ILS";
    public DateTime AcquiredAt { get; set; }
    public Guid StatusId { get; set; }
    public string? LocationNotes { get; set; }
    public Guid? PhotoMediaId { get; set; }
}

public class DeleteAssetCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}

public class CreateAssetCategoryCommand : IRequest<AssetCategoryDto>
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? NumberingPrefix { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateAssetCategoryCommand : IRequest<AssetCategoryDto>
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? NumberingPrefix { get; set; }
    public bool IsActive { get; set; } = true;
}

public class DeleteAssetCategoryCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}

public class UpdateAssetNumberingSettingCommand : IRequest<AssetNumberingSettingDto>
{
    public string Prefix { get; set; } = "A-";
    public int PadWidth { get; set; } = 4;
    public long NextNumber { get; set; } = 1;
}
