using MediatR;

namespace Maba.Application.Features.Cms.Pages.Commands;

public class DeletePageCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}

