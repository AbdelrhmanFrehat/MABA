using Maba.Application.Features.Assets.DTOs;
using Maba.Domain.Assets;

namespace Maba.Application.Features.Assets.Handlers;

internal static class AssetMapping
{
    public static AssetDto ToDto(this Asset a) => new()
    {
        Id = a.Id,
        AssetNumber = a.AssetNumber,
        NameEn = a.NameEn,
        NameAr = a.NameAr,
        DescriptionEn = a.DescriptionEn,
        DescriptionAr = a.DescriptionAr,
        AssetCategoryId = a.AssetCategoryId,
        AssetCategoryNameEn = a.AssetCategory?.NameEn,
        AssetCategoryNameAr = a.AssetCategory?.NameAr,
        InvestorUserId = a.InvestorUserId,
        InvestorUserFullName = a.InvestorUser?.FullName,
        AcquisitionConditionId = a.AcquisitionConditionId,
        AcquisitionConditionKey = a.AcquisitionCondition?.Key,
        AcquisitionConditionNameEn = a.AcquisitionCondition?.NameEn,
        AcquisitionConditionNameAr = a.AcquisitionCondition?.NameAr,
        ConditionNotes = a.ConditionNotes,
        ApproximatePrice = a.ApproximatePrice,
        Currency = a.Currency,
        AcquiredAt = a.AcquiredAt,
        StatusId = a.StatusId,
        StatusKey = a.Status?.Key,
        StatusNameEn = a.Status?.NameEn,
        StatusNameAr = a.Status?.NameAr,
        LocationNotes = a.LocationNotes,
        PhotoMediaId = a.PhotoMediaId,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };

    public static AssetCategoryDto ToDto(this AssetCategory c) => new()
    {
        Id = c.Id,
        NameEn = c.NameEn,
        NameAr = c.NameAr,
        NumberingPrefix = c.NumberingPrefix,
        IsActive = c.IsActive,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };

    public static AssetNumberingSettingDto ToDto(this AssetNumberingSetting s) => new()
    {
        Id = s.Id,
        Prefix = s.Prefix,
        PadWidth = s.PadWidth,
        NextNumber = s.NextNumber
    };
}
