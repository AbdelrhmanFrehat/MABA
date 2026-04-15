using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Assets.DTOs;
using Maba.Application.Features.Assets.Queries;
using Maba.Domain.Assets;

namespace Maba.Application.Features.Assets.Handlers;

public class GetAssetsQueryHandler : IRequestHandler<GetAssetsQuery, List<AssetDto>>
{
    private readonly IApplicationDbContext _context;
    public GetAssetsQueryHandler(IApplicationDbContext context) { _context = context; }

    public async Task<List<AssetDto>> Handle(GetAssetsQuery request, CancellationToken cancellationToken)
    {
        var q = _context.Set<Asset>()
            .Include(a => a.AssetCategory)
            .Include(a => a.InvestorUser)
            .Include(a => a.AcquisitionCondition)
            .Include(a => a.Status)
            .AsQueryable();

        if (request.CategoryId.HasValue) q = q.Where(a => a.AssetCategoryId == request.CategoryId.Value);
        if (request.InvestorUserId.HasValue) q = q.Where(a => a.InvestorUserId == request.InvestorUserId.Value);
        if (request.StatusId.HasValue) q = q.Where(a => a.StatusId == request.StatusId.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            q = q.Where(a => a.AssetNumber.ToLower().Contains(s)
                || a.NameEn.ToLower().Contains(s)
                || a.NameAr.Contains(request.Search));
        }

        var list = await q.OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);
        return list.Select(a => a.ToDto()).ToList();
    }
}

public class GetAssetByIdQueryHandler : IRequestHandler<GetAssetByIdQuery, AssetDto?>
{
    private readonly IApplicationDbContext _context;
    public GetAssetByIdQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<AssetDto?> Handle(GetAssetByIdQuery request, CancellationToken cancellationToken)
    {
        var a = await _context.Set<Asset>()
            .Include(x => x.AssetCategory)
            .Include(x => x.InvestorUser)
            .Include(x => x.AcquisitionCondition)
            .Include(x => x.Status)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        return a?.ToDto();
    }
}

public class GetAssetByNumberQueryHandler : IRequestHandler<GetAssetByNumberQuery, AssetDto?>
{
    private readonly IApplicationDbContext _context;
    public GetAssetByNumberQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<AssetDto?> Handle(GetAssetByNumberQuery request, CancellationToken cancellationToken)
    {
        var a = await _context.Set<Asset>()
            .Include(x => x.AssetCategory)
            .Include(x => x.InvestorUser)
            .Include(x => x.AcquisitionCondition)
            .Include(x => x.Status)
            .FirstOrDefaultAsync(x => x.AssetNumber == request.AssetNumber, cancellationToken);
        return a?.ToDto();
    }
}

public class GetAssetCategoriesQueryHandler : IRequestHandler<GetAssetCategoriesQuery, List<AssetCategoryDto>>
{
    private readonly IApplicationDbContext _context;
    public GetAssetCategoriesQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<List<AssetCategoryDto>> Handle(GetAssetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var list = await _context.Set<AssetCategory>().OrderBy(c => c.NameEn).ToListAsync(cancellationToken);
        return list.Select(c => c.ToDto()).ToList();
    }
}

public class GetAssetNumberingSettingQueryHandler : IRequestHandler<GetAssetNumberingSettingQuery, AssetNumberingSettingDto>
{
    private readonly IApplicationDbContext _context;
    public GetAssetNumberingSettingQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<AssetNumberingSettingDto> Handle(GetAssetNumberingSettingQuery request, CancellationToken cancellationToken)
    {
        var s = await _context.Set<AssetNumberingSetting>().FirstOrDefaultAsync(cancellationToken);
        if (s == null) return new AssetNumberingSettingDto();
        return s.ToDto();
    }
}
