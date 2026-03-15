using MediatR;
using Maba.Application.Features.Common.Settings.DTOs;

namespace Maba.Application.Features.Common.Settings.Queries;

public class GetSystemSettingsQuery : IRequest<List<SystemSettingDto>>
{
    public string? Category { get; set; }
    public bool? IsPublic { get; set; }
}

