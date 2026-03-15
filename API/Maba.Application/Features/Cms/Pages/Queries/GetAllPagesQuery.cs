using MediatR;
using Maba.Application.Features.Cms.DTOs;

namespace Maba.Application.Features.Cms.Pages.Queries;

public class GetAllPagesQuery : IRequest<List<PageDto>>
{
    public bool? IsActive { get; set; }
}

