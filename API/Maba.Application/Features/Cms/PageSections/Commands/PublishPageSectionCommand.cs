using MediatR;
using Maba.Application.Features.Cms.DTOs;

namespace Maba.Application.Features.Cms.PageSections.Commands;

public class PublishPageSectionCommand : IRequest<PageSectionPublishedDto>
{
    public Guid DraftId { get; set; }
    public Guid? PublishedByUserId { get; set; }
}

