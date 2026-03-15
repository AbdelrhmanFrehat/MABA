using MediatR;
using Maba.Application.Features.Projects.DTOs;
using Maba.Domain.Projects;

namespace Maba.Application.Features.Projects.Queries;

public class GetProjectsQuery : IRequest<ProjectsListResponse>
{
    public string? SearchTerm { get; set; }
    public ProjectCategory? Category { get; set; }
    public ProjectStatus? Status { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
