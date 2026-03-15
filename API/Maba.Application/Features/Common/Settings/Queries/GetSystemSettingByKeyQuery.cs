using MediatR;
using Maba.Application.Features.Common.Settings.DTOs;

namespace Maba.Application.Features.Common.Settings.Queries;

public class GetSystemSettingByKeyQuery : IRequest<SystemSettingDto>
{
    public string Key { get; set; } = string.Empty;
}

