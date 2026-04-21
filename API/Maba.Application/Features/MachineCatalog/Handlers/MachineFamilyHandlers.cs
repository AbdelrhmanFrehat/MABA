using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.MachineCatalog.DTOs;
using Maba.Domain.MachineCatalog;

namespace Maba.Application.Features.MachineCatalog.Handlers;

// ── Queries ───────────────────────────────────────────────────────────────

public record GetMachineFamiliesQuery(Guid? CategoryId = null, bool? IsActive = null) : IRequest<List<MachineFamilyDto>>;

public class GetMachineFamiliesQueryHandler : IRequestHandler<GetMachineFamiliesQuery, List<MachineFamilyDto>>
{
    private readonly IApplicationDbContext _ctx;
    public GetMachineFamiliesQueryHandler(IApplicationDbContext ctx) => _ctx = ctx;

    public async Task<List<MachineFamilyDto>> Handle(GetMachineFamiliesQuery request, CancellationToken ct)
    {
        var q = _ctx.Set<MachineFamily>().Include(f => f.Category).AsQueryable();

        if (request.CategoryId.HasValue)
            q = q.Where(f => f.CategoryId == request.CategoryId.Value);

        if (request.IsActive.HasValue)
            q = q.Where(f => f.IsActive == request.IsActive.Value);

        return await q.OrderBy(f => f.SortOrder).ThenBy(f => f.DisplayNameEn)
            .Select(f => MapToDto(f))
            .ToListAsync(ct);
    }

    internal static MachineFamilyDto MapToDto(MachineFamily f) => new()
    {
        Id = f.Id,
        CategoryId = f.CategoryId,
        CategoryDisplayNameEn = f.Category?.DisplayNameEn,
        Code = f.Code,
        DisplayNameEn = f.DisplayNameEn,
        DisplayNameAr = f.DisplayNameAr,
        DescriptionEn = f.DescriptionEn,
        DescriptionAr = f.DescriptionAr,
        Manufacturer = f.Manufacturer,
        LogoUrl = f.LogoUrl,
        IsActive = f.IsActive,
        SortOrder = f.SortOrder,
        CreatedAt = f.CreatedAt,
        UpdatedAt = f.UpdatedAt
    };
}

public record GetMachineFamilyByIdQuery(Guid Id) : IRequest<MachineFamilyDto>;

public class GetMachineFamilyByIdQueryHandler : IRequestHandler<GetMachineFamilyByIdQuery, MachineFamilyDto>
{
    private readonly IApplicationDbContext _ctx;
    public GetMachineFamilyByIdQueryHandler(IApplicationDbContext ctx) => _ctx = ctx;

    public async Task<MachineFamilyDto> Handle(GetMachineFamilyByIdQuery request, CancellationToken ct)
    {
        var entity = await _ctx.Set<MachineFamily>()
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("Machine family not found.");
        return GetMachineFamiliesQueryHandler.MapToDto(entity);
    }
}

// ── Commands ──────────────────────────────────────────────────────────────

public record CreateMachineFamilyCommand(CreateMachineFamilyRequest Request) : IRequest<MachineFamilyDto>;

public class CreateMachineFamilyCommandHandler : IRequestHandler<CreateMachineFamilyCommand, MachineFamilyDto>
{
    private readonly IApplicationDbContext _ctx;
    public CreateMachineFamilyCommandHandler(IApplicationDbContext ctx) => _ctx = ctx;

    public async Task<MachineFamilyDto> Handle(CreateMachineFamilyCommand request, CancellationToken ct)
    {
        var r = request.Request;
        var categoryExists = await _ctx.Set<MachineCategory>().AnyAsync(c => c.Id == r.CategoryId, ct);
        if (!categoryExists) throw new KeyNotFoundException("Machine category not found.");

        var entity = new MachineFamily
        {
            CategoryId = r.CategoryId,
            Code = r.Code.Trim().ToUpperInvariant(),
            DisplayNameEn = r.DisplayNameEn.Trim(),
            DisplayNameAr = r.DisplayNameAr.Trim(),
            DescriptionEn = r.DescriptionEn?.Trim(),
            DescriptionAr = r.DescriptionAr?.Trim(),
            Manufacturer = string.IsNullOrWhiteSpace(r.Manufacturer) ? "MABA" : r.Manufacturer.Trim(),
            LogoUrl = r.LogoUrl?.Trim(),
            IsActive = r.IsActive,
            SortOrder = r.SortOrder
        };
        _ctx.Set<MachineFamily>().Add(entity);
        await _ctx.SaveChangesAsync(ct);

        await _ctx.Set<MachineFamily>().Entry(entity).Reference(f => f.Category).LoadAsync(ct);
        return GetMachineFamiliesQueryHandler.MapToDto(entity);
    }
}

public record UpdateMachineFamilyCommand(UpdateMachineFamilyRequest Request) : IRequest<MachineFamilyDto>;

public class UpdateMachineFamilyCommandHandler : IRequestHandler<UpdateMachineFamilyCommand, MachineFamilyDto>
{
    private readonly IApplicationDbContext _ctx;
    public UpdateMachineFamilyCommandHandler(IApplicationDbContext ctx) => _ctx = ctx;

    public async Task<MachineFamilyDto> Handle(UpdateMachineFamilyCommand request, CancellationToken ct)
    {
        var r = request.Request;
        var entity = await _ctx.Set<MachineFamily>()
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id == r.Id, ct)
            ?? throw new KeyNotFoundException("Machine family not found.");

        if (entity.CategoryId != r.CategoryId)
        {
            var categoryExists = await _ctx.Set<MachineCategory>().AnyAsync(c => c.Id == r.CategoryId, ct);
            if (!categoryExists) throw new KeyNotFoundException("Machine category not found.");
            entity.CategoryId = r.CategoryId;
        }

        entity.Code = r.Code.Trim().ToUpperInvariant();
        entity.DisplayNameEn = r.DisplayNameEn.Trim();
        entity.DisplayNameAr = r.DisplayNameAr.Trim();
        entity.DescriptionEn = r.DescriptionEn?.Trim();
        entity.DescriptionAr = r.DescriptionAr?.Trim();
        entity.Manufacturer = string.IsNullOrWhiteSpace(r.Manufacturer) ? "MABA" : r.Manufacturer.Trim();
        entity.LogoUrl = r.LogoUrl?.Trim();
        entity.IsActive = r.IsActive;
        entity.SortOrder = r.SortOrder;

        await _ctx.SaveChangesAsync(ct);
        return GetMachineFamiliesQueryHandler.MapToDto(entity);
    }
}

public record ToggleMachineFamilyActiveCommand(Guid Id) : IRequest<bool>;

public class ToggleMachineFamilyActiveCommandHandler : IRequestHandler<ToggleMachineFamilyActiveCommand, bool>
{
    private readonly IApplicationDbContext _ctx;
    public ToggleMachineFamilyActiveCommandHandler(IApplicationDbContext ctx) => _ctx = ctx;

    public async Task<bool> Handle(ToggleMachineFamilyActiveCommand request, CancellationToken ct)
    {
        var entity = await _ctx.Set<MachineFamily>().FirstOrDefaultAsync(f => f.Id == request.Id, ct);
        if (entity == null) return false;
        entity.IsActive = !entity.IsActive;
        await _ctx.SaveChangesAsync(ct);
        return true;
    }
}
