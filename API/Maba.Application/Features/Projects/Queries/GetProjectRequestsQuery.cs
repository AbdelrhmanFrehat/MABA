using MediatR;
using Maba.Application.Features.Projects.DTOs;
using Maba.Domain.Projects;

namespace Maba.Application.Features.Projects.Queries;

public class GetProjectRequestsQuery : IRequest<List<ProjectRequestDto>>
{
    public ProjectRequestStatus? Status { get; set; }
    public ProjectRequestType? RequestType { get; set; }
    public string? SearchTerm { get; set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
}
