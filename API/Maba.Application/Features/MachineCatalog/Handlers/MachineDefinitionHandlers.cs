using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.MachineCatalog.DTOs;
using Maba.Domain.MachineCatalog;
using Maba.Domain.MachineCatalog.Sections;

namespace Maba.Application.Features.MachineCatalog.Handlers;

// ── Queries ───────────────────────────────────────────────────────────────

public record GetMachineDefinitionsQuery(MachineDefinitionListQuery Params) : IRequest<List<MachineDefinitionSummaryDto>>;

public class GetMachineDefinitionsQueryHandler : IRequestHandler<GetMachineDefinitionsQuery, List<MachineDefinitionSummaryDto>>
{
    private readonly IApplicationDbContext _ctx;
    public GetMachineDefinitionsQueryHandler(IApplicationDbContext ctx) => _ctx = ctx;

    public async Task<List<MachineDefinitionSummaryDto>> Handle(GetMachineDefinitionsQuery request, CancellationToken ct)
    {
        var p = request.Params;
        var q = _ctx.Set<MachineDefinition>()
            .Include(d => d.Category)
            .Include(d => d.Family)
            .AsQueryable();

        if (p.ActiveOnly)
            q = q.Where(d => d.IsActive);

        if (!p.IncludeDeprecated)
            q = q.Where(d => !d.IsDeprecated);

        if (!p.AdminMode)
            q = q.Where(d => d.IsPublic);

        if (p.CategoryId.HasValue)
            q = q.Where(d => d.CategoryId == p.CategoryId.Value);

        if (p.FamilyId.HasValue)
            q = q.Where(d => d.FamilyId == p.FamilyId.Value);

        if (!string.IsNullOrWhiteSpace(p.Search))
        {
            var s = p.Search.Trim().ToLower();
            q = q.Where(d => d.DisplayNameEn.ToLower().Contains(s)
                           || d.DisplayNameAr.ToLower().Contains(s)
                           || d.Code.ToLower().Contains(s));
        }

        var results = await q.OrderBy(d => d.SortOrder).ThenBy(d => d.DisplayNameEn).ToListAsync(ct);
        return results.Select(d => MapToSummary(d)).ToList();
    }

    internal static MachineDefinitionSummaryDto MapToSummary(MachineDefinition d)
    {
        var tags = DeserializeTags(d.TagsJson);
        return new MachineDefinitionSummaryDto
        {
            Id = d.Id,
            Code = d.Code,
            Version = d.Version,
            CategoryId = d.CategoryId,
            CategoryDisplayNameEn = d.Category?.DisplayNameEn,
            FamilyId = d.FamilyId,
            FamilyDisplayNameEn = d.Family?.DisplayNameEn,
            DisplayNameEn = d.DisplayNameEn,
            DisplayNameAr = d.DisplayNameAr,
            Manufacturer = d.Manufacturer,
            IsActive = d.IsActive,
            IsPublic = d.IsPublic,
            IsDeprecated = d.IsDeprecated,
            DeprecationNote = d.DeprecationNote,
            SortOrder = d.SortOrder,
            ReleasedAt = d.ReleasedAt,
            Tags = tags,
            ImageUrl = d.ImageUrl,
            ThumbnailUrl = d.ThumbnailUrl,
            DefaultDriverType = d.RuntimeBinding.DefaultDriverType.ToString(),
            SupportedSetupModes = d.RuntimeBinding.SupportedSetupModes.Select(m => m.ToString()).ToList(),
            RuntimeUiVariant = d.RuntimeBinding.RuntimeUiVariant,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        };
    }

    internal static List<string> DeserializeTags(string json)
    {
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? new(); }
        catch { return new(); }
    }
}

public record GetMachineDefinitionByIdQuery(Guid Id, bool IncludeInternalNotes = false) : IRequest<MachineDefinitionDto>;

public class GetMachineDefinitionByIdQueryHandler : IRequestHandler<GetMachineDefinitionByIdQuery, MachineDefinitionDto>
{
    private readonly IApplicationDbContext _ctx;
    public GetMachineDefinitionByIdQueryHandler(IApplicationDbContext ctx) => _ctx = ctx;

    public async Task<MachineDefinitionDto> Handle(GetMachineDefinitionByIdQuery request, CancellationToken ct)
    {
        var d = await _ctx.Set<MachineDefinition>()
            .Include(x => x.Category)
            .Include(x => x.Family)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("Machine definition not found.");

        return MapToDto(d, request.IncludeInternalNotes);
    }

    internal static MachineDefinitionDto MapToDto(MachineDefinition d, bool includeInternalNotes = false) => new()
    {
        Id = d.Id,
        Code = d.Code,
        Version = d.Version,
        RevisionNote = d.RevisionNote,
        CategoryId = d.CategoryId,
        CategoryDisplayNameEn = d.Category?.DisplayNameEn,
        FamilyId = d.FamilyId,
        FamilyDisplayNameEn = d.Family?.DisplayNameEn,
        DisplayNameEn = d.DisplayNameEn,
        DisplayNameAr = d.DisplayNameAr,
        DescriptionEn = d.DescriptionEn,
        DescriptionAr = d.DescriptionAr,
        Manufacturer = d.Manufacturer,
        Tags = GetMachineDefinitionsQueryHandler.DeserializeTags(d.TagsJson),
        IsActive = d.IsActive,
        IsPublic = d.IsPublic,
        IsDeprecated = d.IsDeprecated,
        DeprecationNote = d.DeprecationNote,
        SortOrder = d.SortOrder,
        ReleasedAt = d.ReleasedAt,
        ImageUrl = d.ImageUrl,
        ThumbnailUrl = d.ThumbnailUrl,
        InternalNotes = includeInternalNotes ? d.InternalNotes : null,
        CreatedAt = d.CreatedAt,
        UpdatedAt = d.UpdatedAt,
        RuntimeBinding = d.RuntimeBinding,
        AxisConfig = d.AxisConfig,
        Workspace = d.Workspace,
        MotionDefaults = d.MotionDefaults,
        ConnectionDefaults = d.ConnectionDefaults,
        Capabilities = d.Capabilities,
        FileSupport = d.FileSupport,
        Visualization = d.Visualization,
        ProfileRules = d.ProfileRules
    };
}

public record GetMachineDefinitionCapabilitiesQuery(Guid Id) : IRequest<CapabilitiesSection>;

public class GetMachineDefinitionCapabilitiesQueryHandler : IRequestHandler<GetMachineDefinitionCapabilitiesQuery, CapabilitiesSection>
{
    private readonly IApplicationDbContext _ctx;
    public GetMachineDefinitionCapabilitiesQueryHandler(IApplicationDbContext ctx) => _ctx = ctx;

    public async Task<CapabilitiesSection> Handle(GetMachineDefinitionCapabilitiesQuery request, CancellationToken ct)
    {
        var d = await _ctx.Set<MachineDefinition>()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive, ct)
            ?? throw new KeyNotFoundException("Machine definition not found.");
        return d.Capabilities;
    }
}

// ── Commands ──────────────────────────────────────────────────────────────

public record CreateMachineDefinitionCommand(CreateMachineDefinitionRequest Request) : IRequest<MachineDefinitionDto>;

public class CreateMachineDefinitionCommandHandler : IRequestHandler<CreateMachineDefinitionCommand, MachineDefinitionDto>
{
    private readonly IApplicationDbContext _ctx;
    public CreateMachineDefinitionCommandHandler(IApplicationDbContext ctx) => _ctx = ctx;

    public async Task<MachineDefinitionDto> Handle(CreateMachineDefinitionCommand request, CancellationToken ct)
    {
        var r = request.Request;
        ValidateCrossSection(r.RuntimeBinding, r.AxisConfig, r.ConnectionDefaults);

        var entity = new MachineDefinition
        {
            Code = r.Code.Trim().ToUpperInvariant(),
            Version = r.Version.Trim(),
            RevisionNote = r.RevisionNote?.Trim(),
            CategoryId = r.CategoryId,
            FamilyId = r.FamilyId,
            DisplayNameEn = r.DisplayNameEn.Trim(),
            DisplayNameAr = r.DisplayNameAr.Trim(),
            DescriptionEn = r.DescriptionEn?.Trim(),
            DescriptionAr = r.DescriptionAr?.Trim(),
            Manufacturer = string.IsNullOrWhiteSpace(r.Manufacturer) ? "MABA" : r.Manufacturer.Trim(),
            TagsJson = JsonSerializer.Serialize(r.Tags ?? new()),
            IsActive = r.IsActive,
            IsPublic = r.IsPublic,
            IsDeprecated = r.IsDeprecated,
            DeprecationNote = r.DeprecationNote?.Trim(),
            SortOrder = r.SortOrder,
            ReleasedAt = r.ReleasedAt,
            ImageUrl = string.IsNullOrWhiteSpace(r.ImageUrl) ? null : r.ImageUrl.Trim(),
            ThumbnailUrl = string.IsNullOrWhiteSpace(r.ThumbnailUrl) ? null : r.ThumbnailUrl.Trim(),
            InternalNotes = r.InternalNotes?.Trim(),
            RuntimeBinding = r.RuntimeBinding,
            AxisConfig = r.AxisConfig,
            Workspace = r.Workspace,
            MotionDefaults = r.MotionDefaults,
            ConnectionDefaults = r.ConnectionDefaults,
            Capabilities = r.Capabilities,
            FileSupport = r.FileSupport,
            Visualization = r.Visualization,
            ProfileRules = r.ProfileRules
        };

        _ctx.Set<MachineDefinition>().Add(entity);
        await _ctx.SaveChangesAsync(ct);

        await _ctx.Set<MachineDefinition>().Entry(entity).Reference(x => x.Category).LoadAsync(ct);
        await _ctx.Set<MachineDefinition>().Entry(entity).Reference(x => x.Family).LoadAsync(ct);
        return GetMachineDefinitionByIdQueryHandler.MapToDto(entity, includeInternalNotes: true);
    }

    private static void ValidateCrossSection(
        RuntimeBindingSection runtime, AxisConfigSection axis, ConnectionDefaultsSection conn)
    {
        if (!runtime.SupportedDriverTypes.Contains(runtime.DefaultDriverType))
            throw new InvalidOperationException("defaultDriverType must be in supportedDriverTypes.");

        if (!conn.SupportedBaudRates.Contains(conn.DefaultBaudRate))
            throw new InvalidOperationException("defaultBaudRate must be in supportedBaudRates.");

        if (axis.SupportedAxes.Count != axis.AxisCount)
            throw new InvalidOperationException("axisCount must match the number of entries in supportedAxes.");
    }
}

public record UpdateMachineDefinitionCommand(UpdateMachineDefinitionRequest Request) : IRequest<MachineDefinitionDto>;

public class UpdateMachineDefinitionCommandHandler : IRequestHandler<UpdateMachineDefinitionCommand, MachineDefinitionDto>
{
    private readonly IApplicationDbContext _ctx;
    public UpdateMachineDefinitionCommandHandler(IApplicationDbContext ctx) => _ctx = ctx;

    public async Task<MachineDefinitionDto> Handle(UpdateMachineDefinitionCommand request, CancellationToken ct)
    {
        var r = request.Request;
        var entity = await _ctx.Set<MachineDefinition>()
            .Include(x => x.Category)
            .Include(x => x.Family)
            .FirstOrDefaultAsync(x => x.Id == r.Id, ct)
            ?? throw new KeyNotFoundException("Machine definition not found.");

        // Cross-section validation
        if (!r.RuntimeBinding.SupportedDriverTypes.Contains(r.RuntimeBinding.DefaultDriverType))
            throw new InvalidOperationException("defaultDriverType must be in supportedDriverTypes.");
        if (!r.ConnectionDefaults.SupportedBaudRates.Contains(r.ConnectionDefaults.DefaultBaudRate))
            throw new InvalidOperationException("defaultBaudRate must be in supportedBaudRates.");
        if (r.AxisConfig.SupportedAxes.Count != r.AxisConfig.AxisCount)
            throw new InvalidOperationException("axisCount must match the number of entries in supportedAxes.");

        entity.Code = r.Code.Trim().ToUpperInvariant();
        entity.Version = r.Version.Trim();
        entity.RevisionNote = r.RevisionNote?.Trim();
        entity.CategoryId = r.CategoryId;
        entity.FamilyId = r.FamilyId;
        entity.DisplayNameEn = r.DisplayNameEn.Trim();
        entity.DisplayNameAr = r.DisplayNameAr.Trim();
        entity.DescriptionEn = r.DescriptionEn?.Trim();
        entity.DescriptionAr = r.DescriptionAr?.Trim();
        entity.Manufacturer = string.IsNullOrWhiteSpace(r.Manufacturer) ? "MABA" : r.Manufacturer.Trim();
        entity.TagsJson = JsonSerializer.Serialize(r.Tags ?? new());
        entity.IsActive = r.IsActive;
        entity.IsPublic = r.IsPublic;
        entity.IsDeprecated = r.IsDeprecated;
        entity.DeprecationNote = r.DeprecationNote?.Trim();
        entity.SortOrder = r.SortOrder;
        entity.ReleasedAt = r.ReleasedAt;
        entity.ImageUrl = string.IsNullOrWhiteSpace(r.ImageUrl) ? null : r.ImageUrl.Trim();
        entity.ThumbnailUrl = string.IsNullOrWhiteSpace(r.ThumbnailUrl) ? null : r.ThumbnailUrl.Trim();
        entity.InternalNotes = r.InternalNotes?.Trim();
        entity.RuntimeBinding = r.RuntimeBinding;
        entity.AxisConfig = r.AxisConfig;
        entity.Workspace = r.Workspace;
        entity.MotionDefaults = r.MotionDefaults;
        entity.ConnectionDefaults = r.ConnectionDefaults;
        entity.Capabilities = r.Capabilities;
        entity.FileSupport = r.FileSupport;
        entity.Visualization = r.Visualization;
        entity.ProfileRules = r.ProfileRules;

        await _ctx.SaveChangesAsync(ct);
        return GetMachineDefinitionByIdQueryHandler.MapToDto(entity, includeInternalNotes: true);
    }
}

public record PatchMachineDefinitionStatusCommand(Guid Id, PatchMachineDefinitionStatusRequest Request) : IRequest<bool>;

public class PatchMachineDefinitionStatusCommandHandler : IRequestHandler<PatchMachineDefinitionStatusCommand, bool>
{
    private readonly IApplicationDbContext _ctx;
    public PatchMachineDefinitionStatusCommandHandler(IApplicationDbContext ctx) => _ctx = ctx;

    public async Task<bool> Handle(PatchMachineDefinitionStatusCommand request, CancellationToken ct)
    {
        var entity = await _ctx.Set<MachineDefinition>().FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity == null) return false;

        var r = request.Request;
        if (r.IsActive.HasValue) entity.IsActive = r.IsActive.Value;
        if (r.IsPublic.HasValue) entity.IsPublic = r.IsPublic.Value;
        if (r.IsDeprecated.HasValue) entity.IsDeprecated = r.IsDeprecated.Value;
        if (r.DeprecationNote != null) entity.DeprecationNote = r.DeprecationNote.Trim();

        await _ctx.SaveChangesAsync(ct);
        return true;
    }
}

public record SoftDeleteMachineDefinitionCommand(Guid Id) : IRequest<bool>;

public class SoftDeleteMachineDefinitionCommandHandler : IRequestHandler<SoftDeleteMachineDefinitionCommand, bool>
{
    private readonly IApplicationDbContext _ctx;
    public SoftDeleteMachineDefinitionCommandHandler(IApplicationDbContext ctx) => _ctx = ctx;

    public async Task<bool> Handle(SoftDeleteMachineDefinitionCommand request, CancellationToken ct)
    {
        var entity = await _ctx.Set<MachineDefinition>().FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity == null) return false;
        entity.IsActive = false;
        await _ctx.SaveChangesAsync(ct);
        return true;
    }
}
