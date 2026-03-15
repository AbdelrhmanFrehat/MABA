using MediatR;
using Maba.Application.Features.Cms.DTOs;

namespace Maba.Application.Features.Cms.Pages.Queries;

public class GetPagePreviewQuery : IRequest<PagePreviewDto>
{
    public Guid PageId { get; set; }
}

