using MediatR;

namespace Maba.Application.Features.Projects.Commands;

public class DeleteProjectCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
