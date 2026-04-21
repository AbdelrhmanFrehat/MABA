using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.MachineCatalog.DTOs;
using Maba.Domain.MachineCatalog;

namespace Maba.Application.Features.MachineCatalog.Handlers;

// ── Queries ───────────────────────────────────────────────────────────────

public record GetMachineCategoriesQuery(bool? IsActive = null) : IRequest<List<MachineCategoryDto>>;

public class GetMachineCategoriesQueryHandler : IRequestHandler<GetMachineCategoriesQuery, List<MachineCategoryDto>>
{
    private readonly IApplicationDbContext _ctx;
    public GetMachineCategoriesQueryHandler(IApplicationDbContext ctx) => _ctx = ctx;

    public async Task<List<MachineCategoryDto>> Handle(GetMachineCategoriesQuery request, CancellationToken ct)
    {
        var q = _ctx.Set<MachineCategory>().AsQueryable();
        if (request.IsActive.HasValue)
            q = q.Where(x => x.IsActive == request.IsActive.Value);

        return await q.OrderBy(x => x.SortOrder).ThenBy(x => x.DisplayNameEn)
            .Select(x => MapToDto(x))
            .ToListAsync(ct);
    }

    internal static MachineCategoryDto MapToDto(MachineCategory x) => new()
    {
        Id = x.Id,
        Code = x.Code,
        DisplayNameEn = x.DisplayNameEn,
        DisplayNameAr = x.DisplayNameAr,
        DescriptionEn = x.DescriptionEn,
        DescriptionAr = x.DescriptionAr,
        IconKey = x.IconKey,
        SortOrder = x.SortOrder,
        IsActive = x.IsActive,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt
    };
}

public record GetMachineCategoryByIdQuery(Guid Id) : IRequest<MachineCategoryDto>;

public class GetMachineCategoryByIdQueryHandler : IRequestHandler<GetMachineCategoryByIdQuery, MachineCategoryDto>
{
    private readonly IApplicationDbContext _ctx;
    public GetMachineCategoryByIdQueryHandler(IApplicationDbContext ctx) => _ctx = ctx;

    public async Task<MachineCategoryDto> Handle(GetMachineCategoryByIdQuery request, CancellationToken ct)
    {
        var entity = await _ctx.Set<MachineCategory>().FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("Machine category not found.");
        return GetMachineCategoriesQueryHandler.MapToDto(entity);
    }
}

// ── Commands ──────────────────────────────────────────────────────────────

public record CreateMachineCategoryCommand(CreateMachineCategoryRequest Request) : IRequest<MachineCategoryDto>;

public class CreateMachineCategoryCommandHandler : IRequestHandler<CreateMachineCategoryCommand, MachineCategoryDto>
{
    private readonly IApplicationDbContext _ctx;
    public CreateMachineCategoryCommandHandler(IApplicationDbContext ctx) => _ctx = ctx;

    public async Task<MachineCategoryDto> Handle(CreateMachineCategoryCommand request, CancellationToken ct)
    {
        var r = request.Request;
        var entity = new MachineCategory
        {
            Code = r.Code.Trim().ToUpperInvariant(),
            DisplayNameEn = r.DisplayNameEn.Trim(),
            DisplayNameAr = r.DisplayNameAr.Trim(),
            DescriptionEn = r.DescriptionEn?.Trim(),
            DescriptionAr = r.DescriptionAr?.Trim(),
            IconKey = r.IconKey?.Trim(),
            SortOrder = r.SortOrder,
            IsActive = r.IsActive
        };
        _ctx.Set<MachineCategory>().Add(entity);
        await _ctx.SaveChangesAsync(ct);
        return GetMachineCategoriesQueryHandler.MapToDto(entity);
    }
}

public record UpdateMachineCategoryCommand(UpdateMachineCategoryRequest Request) : IRequest<MachineCategoryDto>;

public class UpdateMachineCategoryCommandHandler : IRequestHandler<UpdateMachineCategoryCommand, MachineCategoryDto>
{
    private readonly IApplicationDbContext _ctx;
    public UpdateMachineCategoryCommandHandler(IApplicationDbContext ctx) => _ctx = ctx;

    public async Task<MachineCategoryDto> Handle(UpdateMachineCategoryCommand request, CancellationToken ct)
    {
        var r = request.Request;
        var entity = await _ctx.Set<MachineCategory>().FirstOrDefaultAsync(x => x.Id == r.Id, ct)
            ?? throw new KeyNotFoundException("Machine category not found.");

        entity.Code = r.Code.Trim().ToUpperInvariant();
        entity.DisplayNameEn = r.DisplayNameEn.Trim();
        entity.DisplayNameAr = r.DisplayNameAr.Trim();
        entity.DescriptionEn = r.DescriptionEn?.Trim();
        entity.DescriptionAr = r.DescriptionAr?.Trim();
        entity.IconKey = r.IconKey?.Trim();
        entity.SortOrder = r.SortOrder;
        entity.IsActive = r.IsActive;

        await _ctx.SaveChangesAsync(ct);
        return GetMachineCategoriesQueryHandler.MapToDto(entity);
    }
}

public record ToggleMachineCategoryActiveCommand(Guid Id) : IRequest<bool>;

public class ToggleMachineCategoryActiveCommandHandler : IRequestHandler<ToggleMachineCategoryActiveCommand, bool>
{
    private readonly IApplicationDbContext _ctx;
    public ToggleMachineCategoryActiveCommandHandler(IApplicationDbContext ctx) => _ctx = ctx;

    public async Task<bool> Handle(ToggleMachineCategoryActiveCommand request, CancellationToken ct)
    {
        var entity = await _ctx.Set<MachineCategory>().FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity == null) return false;
        entity.IsActive = !entity.IsActive;
        await _ctx.SaveChangesAsync(ct);
        return true;
    }
}
