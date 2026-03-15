using MediatR;
using Maba.Application.Features.Common.Settings.DTOs;

namespace Maba.Application.Features.Common.Settings.Commands;

public class UpdateSystemSettingCommand : IRequest<SystemSettingDto>
{
    public Guid Id { get; set; }
    public string? Value { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? Category { get; set; }
    public bool? IsPublic { get; set; }
}

