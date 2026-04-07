using MediatR;
using Maba.Application.Features.Projects.DTOs;

namespace Maba.Application.Features.Projects.Queries;

public class GetProjectRequestActivitiesQuery : IRequest<List<ProjectRequestActivityDto>>
{
    public Guid ProjectRequestId { get; set; }
}
