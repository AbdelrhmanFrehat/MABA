using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Projects.DTOs;
using Maba.Domain.Projects;

namespace Maba.Application.Features.Projects.Queries;

public class GetProjectRequestActivitiesQueryHandler
    : IRequestHandler<GetProjectRequestActivitiesQuery, List<ProjectRequestActivityDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProjectRequestActivitiesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectRequestActivityDto>> Handle(
        GetProjectRequestActivitiesQuery request, CancellationToken cancellationToken)
    {
        var activities = await _context.Set<ProjectRequestActivity>()
            .Where(a => a.ProjectRequestId == request.ProjectRequestId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return activities.Select(a => new ProjectRequestActivityDto
        {
            Id = a.Id,
            ActionType = a.ActionType,
            Description = a.Description,
            CreatedBy = a.CreatedBy,
            CreatedAt = a.CreatedAt
        }).ToList();
    }
}
