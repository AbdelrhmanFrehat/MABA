using MediatR;
using Maba.Application.Features.Projects.DTOs;

namespace Maba.Application.Features.Projects.Queries;

public class GetProjectByIdQuery : IRequest<ProjectDto?>
{
    public Guid Id { get; set; }
}
