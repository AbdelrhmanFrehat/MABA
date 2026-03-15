using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Common.Settings.Queries;
using Maba.Application.Features.Common.Settings.DTOs;
using Maba.Domain.Common;

namespace Maba.Application.Features.Common.Settings.Handlers;

public class GetSystemSettingByKeyQueryHandler : IRequestHandler<GetSystemSettingByKeyQuery, SystemSettingDto>
{
    private readonly IApplicationDbContext _context;

    public GetSystemSettingByKeyQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SystemSettingDto> Handle(GetSystemSettingByKeyQuery request, CancellationToken cancellationToken)
    {
        var setting = await _context.Set<SystemSetting>()
            .FirstOrDefaultAsync(s => s.Key == request.Key, cancellationToken);

        if (setting == null)
        {
            throw new KeyNotFoundException($"System setting with key '{request.Key}' not found.");
        }

        return new SystemSettingDto
        {
            Id = setting.Id,
            Key = setting.Key,
            Value = setting.Value,
            DescriptionEn = setting.DescriptionEn,
            DescriptionAr = setting.DescriptionAr,
            Category = setting.Category,
            DataType = setting.DataType,
            IsPublic = setting.IsPublic,
            IsEncrypted = setting.IsEncrypted,
            CreatedAt = setting.CreatedAt,
            UpdatedAt = setting.UpdatedAt
        };
    }
}

