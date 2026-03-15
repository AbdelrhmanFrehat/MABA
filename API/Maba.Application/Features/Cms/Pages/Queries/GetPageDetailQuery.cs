using MediatR;
using Maba.Application.Features.Cms.DTOs;

namespace Maba.Application.Features.Cms.Pages.Queries;

public class GetPageDetailQuery : IRequest<PageDetailDto>
{
    public Guid PageId { get; set; }
}

