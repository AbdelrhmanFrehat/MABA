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
            query = query.Where(r => r.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.WorkflowStatus))
            query = query.Where(r => r.WorkflowStatus == request.WorkflowStatus);

        if (request.RequestType.HasValue)
            query = query.Where(r => r.RequestType == request.RequestType.Value);

        if (!string.IsNullOrWhiteSpace(request.ProjectType))
            query = query.Where(r => r.ProjectType == request.ProjectType);

        if (!string.IsNullOrWhiteSpace(request.MainDomain))
            query = query.Where(r => r.MainDomain == request.MainDomain);

        if (!string.IsNullOrWhiteSpace(request.ProjectStage))
            query = query.Where(r => r.ProjectStage == request.ProjectStage);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(r =>
                r.ReferenceNumber.ToLower().Contains(term) ||
                r.FullName.ToLower().Contains(term) ||
                r.Email.ToLower().Contains(term) ||
                (r.MainDomain != null && r.MainDomain.ToLower().Contains(term)) ||
                (r.ProjectType != null && r.ProjectType.ToLower().Contains(term)) ||
                (r.ProjectStage != null && r.ProjectStage.ToLower().Contains(term)) ||
                (r.Description != null && r.Description.ToLower().Contains(term)) ||
                (r.AssignedToName != null && r.AssignedToName.ToLower().Contains(term)));
        }

        query = query.OrderByDescending(r => r.CreatedAt);

        if (request.Skip.HasValue)
            query = query.Skip(request.Skip.Value);

        if (request.Take.HasValue)
            query = query.Take(request.Take.Value);

        var items = await query.ToListAsync(cancellationToken);

        return items.Select(r => new ProjectRequestDto
        {
            Id = r.Id,
            ReferenceNumber = r.ReferenceNumber,
            FullName = r.FullName,
            Email = r.Email,
            Phone = r.Phone,
            RequestType = r.RequestType,
            ProjectId = r.ProjectId,
            ProjectTitle = r.Project != null ? r.Project.TitleEn : null,
            ProjectType = r.ProjectType,
            MainDomain = r.MainDomain,
            RequiredCapabilities = ProjectRequestSerialization.DeserializeCapabilities(r.RequiredCapabilitiesJson),
            Category = r.Category,
            BudgetRange = r.BudgetRange,
            Timeline = r.Timeline,
            Description = r.Description,
            ProjectDescription = r.Description,
            ProjectStage = r.ProjectStage,
            AttachmentUrl = r.AttachmentUrl,
            AttachmentFileName = r.AttachmentFileName,
            Attachments = ProjectRequestSerialization.DeserializeAttachments(r.AttachmentsJson, r.AttachmentUrl, r.AttachmentFileName),
            Status = r.Status,
            WorkflowStatus = string.IsNullOrWhiteSpace(r.WorkflowStatus) ? "New" : r.WorkflowStatus,
            AdminNotes = r.AdminNotes,
            AssignedToUserId = r.AssignedToUserId,
            AssignedToName = r.AssignedToName,
            Priority = r.Priority,
            TechnicalFeasibility = r.TechnicalFeasibility,
            EstimatedCost = r.EstimatedCost,
            EstimatedTimeline = r.EstimatedTimeline,
            ComplexityLevel = r.ComplexityLevel,
            InternalNotes = r.InternalNotes,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList();
    }
}
