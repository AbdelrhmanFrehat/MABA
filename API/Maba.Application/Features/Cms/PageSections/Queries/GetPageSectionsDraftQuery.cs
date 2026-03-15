using MediatR;
using Maba.Application.Features.Cms.DTOs;

namespace Maba.Application.Features.Cms.PageSections.Queries;

public class GetPageSectionsDraftQuery : IRequest<List<PageSectionDraftDto>>
{
    public Guid PageId { get; set; }
}

