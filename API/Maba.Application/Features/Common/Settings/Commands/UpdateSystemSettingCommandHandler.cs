using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Common.Settings.Commands;
using Maba.Application.Features.Common.Settings.DTOs;
using Maba.Domain.Common;

namespace Maba.Application.Features.Common.Settings.Handlers;

public class UpdateSystemSettingCommandHandler : IRequestHandler<UpdateSystemSettingCommand, SystemSettingDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateSystemSettingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SystemSettingDto> Handle(UpdateSystemSettingCommand request, CancellationToken cancellationToken)
    {
        var setting = await _context.Set<SystemSetting>()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (setting == null)
        {
            throw new KeyNotFoundException("System setting not found.");
        }

        if (request.Value != null)
        {
            setting.Value = request.Value;
        }

        if (request.DescriptionEn != null)
        {
            setting.DescriptionEn = request.DescriptionEn;
        }

        if (request.DescriptionAr != null)
        {
            setting.DescriptionAr = request.DescriptionAr;
        }

        if (request.Category != null)
        {
            setting.Category = request.Category;
        }

        if (request.IsPublic.HasValue)
        {
            setting.IsPublic = request.IsPublic.Value;
        }

        setting.UpdatedAt = DateTime.UtcNow;

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

