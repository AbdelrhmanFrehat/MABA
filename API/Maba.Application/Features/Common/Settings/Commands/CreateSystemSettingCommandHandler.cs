using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Common.Settings.Commands;
using Maba.Application.Features.Common.Settings.DTOs;
using Maba.Domain.Common;

namespace Maba.Application.Features.Common.Settings.Handlers;

public class CreateSystemSettingCommandHandler : IRequestHandler<CreateSystemSettingCommand, SystemSettingDto>
{
    private readonly IApplicationDbContext _context;

    public CreateSystemSettingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SystemSettingDto> Handle(CreateSystemSettingCommand request, CancellationToken cancellationToken)
    {
        // Check if key already exists
        var existing = await _context.Set<SystemSetting>()
            .FirstOrDefaultAsync(s => s.Key == request.Key, cancellationToken);

        if (existing != null)
        {
            throw new InvalidOperationException($"Setting with key '{request.Key}' already exists.");
        }

        var setting = new SystemSetting
        {
            Id = Guid.NewGuid(),
            Key = request.Key,
            Value = request.Value,
            DescriptionEn = request.DescriptionEn,
            DescriptionAr = request.DescriptionAr,
            Category = request.Category,
            DataType = request.DataType ?? "String",
            IsPublic = request.IsPublic,
            IsEncrypted = request.IsEncrypted
        };

        _context.Set<SystemSetting>().Add(setting);
        await _context.SaveChangesAsync(cancellationToken);

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

