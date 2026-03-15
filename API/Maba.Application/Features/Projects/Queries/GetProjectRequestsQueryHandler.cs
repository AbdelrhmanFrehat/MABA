using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Projects.DTOs;
using Maba.Domain.Projects;

namespace Maba.Application.Features.Projects.Queries;

public class GetProjectRequestsQueryHandler : IRequestHandler<GetProjectRequestsQuery, List<ProjectRequestDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProjectRequestsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectRequestDto>> Handle(GetProjectRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<ProjectRequest>()
            .Include(r => r.Project)
            .AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(r => r.Status == request.Status.Value);
        }

        if (request.RequestType.HasValue)
        {
            query = query.Where(r => r.RequestType == request.RequestType.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(r =>
                r.ReferenceNumber.ToLower().Contains(term) ||
                r.FullName.ToLower().Contains(term) ||
                r.Email.ToLower().Contains(term));
        }

        query = query.OrderByDescending(r => r.CreatedAt);

        if (request.Skip.HasValue)
        {
            query = query.Skip(request.Skip.Value);
        }

        if (request.Take.HasValue)
        {
            query = query.Take(request.Take.Value);
        }

        return await query.Select(r => new ProjectRequestDto
        {
            Id = r.Id,
            ReferenceNumber = r.ReferenceNumber,
            FullName = r.FullName,
            Email = r.Email,
            Phone = r.Phone,
            RequestType = r.RequestType,
            ProjectId = r.ProjectId,
            ProjectTitle = r.Project != null ? r.Project.TitleEn : null,
            Category = r.Category,
            BudgetRange = r.BudgetRange,
            Timeline = r.Timeline,
            Description = r.Description,
            AttachmentUrl = r.AttachmentUrl,
            AttachmentFileName = r.AttachmentFileName,
            Status = r.Status,
            AdminNotes = r.AdminNotes,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToListAsync(cancellationToken);
    }
}
