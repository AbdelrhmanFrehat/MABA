using MediatR;
using Maba.Application.Features.Cms.DTOs;

namespace Maba.Application.Features.Cms.Pages.Commands;

public class UnpublishPageCommand : IRequest<PageDto>
{
    public Guid PageId { get; set; }
    public Guid UnpublishedByUserId { get; set; }
}

