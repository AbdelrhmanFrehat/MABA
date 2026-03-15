using MediatR;
using Maba.Application.Features.Common.Settings.DTOs;

namespace Maba.Application.Features.Common.Settings.Commands;

public class CreateSystemSettingCommand : IRequest<SystemSettingDto>
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? Category { get; set; }
    public string? DataType { get; set; } = "String";
    public bool IsPublic { get; set; } = false;
    public bool IsEncrypted { get; set; } = false;
}

