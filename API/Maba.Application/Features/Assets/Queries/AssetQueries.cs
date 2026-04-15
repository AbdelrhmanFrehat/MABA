using MediatR;
using Maba.Application.Features.Assets.DTOs;

namespace Maba.Application.Features.Assets.Queries;

public class GetAssetsQuery : IRequest<List<AssetDto>>
{
    public Guid? CategoryId { get; set; }
    public Guid? InvestorUserId { get; set; }
    public Guid? StatusId { get; set; }
    public string? Search { get; set; }
}

public class GetAssetByIdQuery : IRequest<AssetDto?>
{
    public Guid Id { get; set; }
}

public class GetAssetByNumberQuery : IRequest<AssetDto?>
{
    public string AssetNumber { get; set; } = string.Empty;
}

public class GetAssetCategoriesQuery : IRequest<List<AssetCategoryDto>>
{
}

public class GetAssetNumberingSettingQuery : IRequest<AssetNumberingSettingDto>
{
}
