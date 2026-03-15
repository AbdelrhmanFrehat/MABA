using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Common.Settings.Queries;
using Maba.Application.Features.Common.Settings.DTOs;
using Maba.Domain.Common;

namespace Maba.Application.Features.Common.Settings.Handlers;

public class GetSystemSettingsQueryHandler : IRequestHandler<GetSystemSettingsQuery, List<SystemSettingDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSystemSettingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SystemSettingDto>> Handle(GetSystemSettingsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<SystemSetting>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(s => s.Category == request.Category);
        }

        if (request.IsPublic.HasValue)
        {
            query = query.Where(s => s.IsPublic == request.IsPublic.Value);
        }

        var settings = await query
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Key)
            .ToListAsync(cancellationToken);

        return settings.Select(s => new SystemSettingDto
        {
            Id = s.Id,
            Key = s.Key,
            Value = s.Value,
            DescriptionEn = s.DescriptionEn,
            DescriptionAr = s.DescriptionAr,
            Category = s.Category,
            DataType = s.DataType,
            IsPublic = s.IsPublic,
            IsEncrypted = s.IsEncrypted,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        }).ToList();
    }
}

