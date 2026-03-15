using MediatR;
using Maba.Application.Features.Cms.DTOs;

namespace Maba.Application.Features.Cms.PageSections.Queries;

public class GetPageSectionsPublishedQuery : IRequest<List<PageSectionPublishedDto>>
{
    public Guid PageId { get; set; }
}

