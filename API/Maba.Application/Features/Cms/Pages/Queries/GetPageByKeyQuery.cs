using MediatR;
using Maba.Application.Features.Cms.DTOs;

namespace Maba.Application.Features.Cms.Pages.Queries;

public class GetPageByKeyQuery : IRequest<PageDto>
{
    public string Key { get; set; } = string.Empty;
}

