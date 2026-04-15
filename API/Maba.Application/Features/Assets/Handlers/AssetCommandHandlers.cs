using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Assets.Commands;
using Maba.Application.Features.Assets.DTOs;
using Maba.Domain.Assets;
using Maba.Domain.Users;

namespace Maba.Application.Features.Assets.Handlers;

public class CreateAssetCommandHandler : IRequestHandler<CreateAssetCommand, AssetDto>
{
    private readonly IApplicationDbContext _context;
    public CreateAssetCommandHandler(IApplicationDbContext context) { _context = context; }

    public async Task<AssetDto> Handle(CreateAssetCommand request, CancellationToken cancellationToken)
    {
        // Validate investor has Admin role
        var isAdmin = await _context.Set<UserRole>()
            .Include(ur => ur.Role)
            .AnyAsync(ur => ur.UserId == request.InvestorUserId && ur.Role.Name == "Admin", cancellationToken);
        if (!isAdmin)
            throw new InvalidOperationException("Investor must be a user with the Admin role.");

        var category = await _context.Set<AssetCategory>()
            .FirstOrDefaultAsync(c => c.Id == request.AssetCategoryId, cancellationToken)
            ?? throw new KeyNotFoundException("Asset category not found.");

        // Generate asset number from settings (simple increment; single-process safe).
        var setting = await _context.Set<AssetNumberingSetting>().FirstOrDefaultAsync(cancellationToken);
        if (setting == null)
        {
            setting = new AssetNumberingSetting { Id = Guid.NewGuid(), Prefix = "A-", PadWidth = 4, NextNumber = 1 };
            _context.Set<AssetNumberingSetting>().Add(setting);
        }

        var prefix = !string.IsNullOrWhiteSpace(category.NumberingPrefix)
            ? category.NumberingPrefix!
            : setting.Prefix;
        var number = setting.NextNumber;
        var assetNumber = $"{prefix}{number.ToString().PadLeft(setting.PadWidth, '0')}";
        setting.NextNumber = number + 1;

        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            AssetNumber = assetNumber,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            DescriptionEn = request.DescriptionEn,
            DescriptionAr = request.DescriptionAr,
            AssetCategoryId = request.AssetCategoryId,
            InvestorUserId = request.InvestorUserId,
            AcquisitionConditionId = request.AcquisitionConditionId,
            ConditionNotes = request.ConditionNotes,
            ApproximatePrice = request.ApproximatePrice,
            Currency = request.Currency,
            AcquiredAt = request.AcquiredAt,
            StatusId = request.StatusId,
            LocationNotes = request.LocationNotes,
            PhotoMediaId = request.PhotoMediaId
        };
        _context.Set<Asset>().Add(asset);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload with includes for the DTO
        var saved = await _context.Set<Asset>()
            .Include(a => a.AssetCategory)
            .Include(a => a.InvestorUser)
            .Include(a => a.AcquisitionCondition)
            .Include(a => a.Status)
            .FirstAsync(a => a.Id == asset.Id, cancellationToken);
        return saved.ToDto();
    }
}

public class UpdateAssetCommandHandler : IRequestHandler<UpdateAssetCommand, AssetDto>
{
    private readonly IApplicationDbContext _context;
    public UpdateAssetCommandHandler(IApplicationDbContext context) { _context = context; }

    public async Task<AssetDto> Handle(UpdateAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await _context.Set<Asset>().FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Asset not found.");

        var isAdmin = await _context.Set<UserRole>()
            .Include(ur => ur.Role)
            .AnyAsync(ur => ur.UserId == request.InvestorUserId && ur.Role.Name == "Admin", cancellationToken);
        if (!isAdmin)
            throw new InvalidOperationException("Investor must be a user with the Admin role.");

        asset.NameEn = request.NameEn;
        asset.NameAr = request.NameAr;
        asset.DescriptionEn = request.DescriptionEn;
        asset.DescriptionAr = request.DescriptionAr;
        asset.AssetCategoryId = request.AssetCategoryId;
        asset.InvestorUserId = request.InvestorUserId;
        asset.AcquisitionConditionId = request.AcquisitionConditionId;
        asset.ConditionNotes = request.ConditionNotes;
        asset.ApproximatePrice = request.ApproximatePrice;
        asset.Currency = request.Currency;
        asset.AcquiredAt = request.AcquiredAt;
        asset.StatusId = request.StatusId;
        asset.LocationNotes = request.LocationNotes;
        asset.PhotoMediaId = request.PhotoMediaId;

        await _context.SaveChangesAsync(cancellationToken);

        var saved = await _context.Set<Asset>()
            .Include(a => a.AssetCategory)
            .Include(a => a.InvestorUser)
            .Include(a => a.AcquisitionCondition)
            .Include(a => a.Status)
            .FirstAsync(a => a.Id == asset.Id, cancellationToken);
        return saved.ToDto();
    }
}

public class DeleteAssetCommandHandler : IRequestHandler<DeleteAssetCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    public DeleteAssetCommandHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Unit> Handle(DeleteAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await _context.Set<Asset>().FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Asset not found.");
        _context.Set<Asset>().Remove(asset);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public class CreateAssetCategoryCommandHandler : IRequestHandler<CreateAssetCategoryCommand, AssetCategoryDto>
{
    private readonly IApplicationDbContext _context;
    public CreateAssetCategoryCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<AssetCategoryDto> Handle(CreateAssetCategoryCommand request, CancellationToken cancellationToken)
    {
        var c = new AssetCategory
        {
            Id = Guid.NewGuid(),
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            NumberingPrefix = request.NumberingPrefix,
            IsActive = request.IsActive
        };
        _context.Set<AssetCategory>().Add(c);
        await _context.SaveChangesAsync(cancellationToken);
        return c.ToDto();
    }
}

public class UpdateAssetCategoryCommandHandler : IRequestHandler<UpdateAssetCategoryCommand, AssetCategoryDto>
{
    private readonly IApplicationDbContext _context;
    public UpdateAssetCategoryCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<AssetCategoryDto> Handle(UpdateAssetCategoryCommand request, CancellationToken cancellationToken)
    {
        var c = await _context.Set<AssetCategory>().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Asset category not found.");
        c.NameEn = request.NameEn;
        c.NameAr = request.NameAr;
        c.NumberingPrefix = request.NumberingPrefix;
        c.IsActive = request.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return c.ToDto();
    }
}

public class DeleteAssetCategoryCommandHandler : IRequestHandler<DeleteAssetCategoryCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    public DeleteAssetCategoryCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<Unit> Handle(DeleteAssetCategoryCommand request, CancellationToken cancellationToken)
    {
        var c = await _context.Set<AssetCategory>().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Asset category not found.");
        var hasAssets = await _context.Set<Asset>().AnyAsync(a => a.AssetCategoryId == c.Id, cancellationToken);
        if (hasAssets) throw new InvalidOperationException("Cannot delete category with existing assets.");
        _context.Set<AssetCategory>().Remove(c);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public class UpdateAssetNumberingSettingCommandHandler : IRequestHandler<UpdateAssetNumberingSettingCommand, AssetNumberingSettingDto>
{
    private readonly IApplicationDbContext _context;
    public UpdateAssetNumberingSettingCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<AssetNumberingSettingDto> Handle(UpdateAssetNumberingSettingCommand request, CancellationToken cancellationToken)
    {
        var s = await _context.Set<AssetNumberingSetting>().FirstOrDefaultAsync(cancellationToken);
        if (s == null)
        {
            s = new AssetNumberingSetting { Id = Guid.NewGuid() };
            _context.Set<AssetNumberingSetting>().Add(s);
        }
        s.Prefix = request.Prefix;
        s.PadWidth = request.PadWidth;
        s.NextNumber = request.NextNumber;
        await _context.SaveChangesAsync(cancellationToken);
        return s.ToDto();
    }
}
