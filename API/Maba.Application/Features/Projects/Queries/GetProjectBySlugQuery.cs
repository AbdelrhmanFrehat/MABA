using MediatR;
using Maba.Application.Features.Projects.DTOs;

namespace Maba.Application.Features.Projects.Queries;

public class GetProjectBySlugQuery : IRequest<ProjectDto?>
{
    public string Slug { get; set; } = string.Empty;
}
