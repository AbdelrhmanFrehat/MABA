using MediatR;
using Maba.Domain.Projects;

namespace Maba.Application.Features.Projects.Commands;

public class UpdateProjectRequestCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public ProjectRequestStatus? Status { get; set; }
    public string? AdminNotes { get; set; }
}
