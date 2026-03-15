using MediatR;
using Maba.Application.Features.Cms.DTOs;

namespace Maba.Application.Features.Cms.Pages.Commands;

public class PublishPageCommand : IRequest<PageDto>
{
    public Guid PageId { get; set; }
    public Guid PublishedByUserId { get; set; }
}

