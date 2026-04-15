using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Projects;

namespace Maba.Application.Features.Projects.Commands;

public class UpdateProjectRequestCommandHandler : IRequestHandler<UpdateProjectRequestCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public UpdateProjectRequestCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<bool> Handle(UpdateProjectRequestCommand request, CancellationToken cancellationToken)
    {
        var projectRequest = await _context.Set<ProjectRequest>()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (projectRequest == null) return false;

        if (!string.IsNullOrWhiteSpace(request.WorkflowStatus) &&
            request.WorkflowStatus == ProjectRequestWorkflowStatus.Rejected &&
            string.IsNullOrWhiteSpace(request.RejectionReason))
        {
            throw new InvalidOperationException("A rejection reason is required when rejecting a request.");
        }

        var activities = new List<ProjectRequestActivity>();
        var previousWorkflowStatus = projectRequest.WorkflowStatus;

        // --- Workflow status transition ---
        if (!string.IsNullOrWhiteSpace(request.WorkflowStatus) &&
            request.WorkflowStatus != previousWorkflowStatus)
        {
            ValidateTransition(projectRequest, request.WorkflowStatus);
            projectRequest.WorkflowStatus = request.WorkflowStatus;

            activities.Add(new ProjectRequestActivity
            {
                Id = Guid.NewGuid(),
                ProjectRequestId = projectRequest.Id,
                ActionType = "StatusChanged",
                Description = $"Status changed from {previousWorkflowStatus} to {request.WorkflowStatus}",
                CreatedBy = request.UpdatedBy,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Legacy status (keep in sync for backward compat)
        if (request.Status.HasValue)
            projectRequest.Status = request.Status.Value;

        // --- Contact fields ---
        if (!string.IsNullOrWhiteSpace(request.FullName))
            projectRequest.FullName = request.FullName.Trim();

        if (!string.IsNullOrWhiteSpace(request.Email))
            projectRequest.Email = request.Email.Trim();

        if (request.Phone != null)
            projectRequest.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();

        // --- Request fields ---
        if (request.RequestType.HasValue)
            projectRequest.RequestType = request.RequestType.Value;

        if (request.ProjectId.HasValue)
            projectRequest.ProjectId = request.ProjectId.Value;

        if (request.ProjectType != null)
            projectRequest.ProjectType = string.IsNullOrWhiteSpace(request.ProjectType) ? null : request.ProjectType.Trim();

        if (request.MainDomain != null)
            projectRequest.MainDomain = string.IsNullOrWhiteSpace(request.MainDomain) ? null : request.MainDomain.Trim();

        if (request.RequiredCapabilities != null)
            projectRequest.RequiredCapabilitiesJson = ProjectRequestSerialization.SerializeCapabilities(request.RequiredCapabilities);

        if (request.Category.HasValue)
            projectRequest.Category = request.Category.Value;

        if (request.BudgetRange != null)
            projectRequest.BudgetRange = string.IsNullOrWhiteSpace(request.BudgetRange) ? null : request.BudgetRange.Trim();

        if (request.Timeline != null)
            projectRequest.Timeline = string.IsNullOrWhiteSpace(request.Timeline) ? null : request.Timeline.Trim();

        if (request.ProjectStage != null)
            projectRequest.ProjectStage = string.IsNullOrWhiteSpace(request.ProjectStage) ? null : request.ProjectStage.Trim();

        if (request.Description != null)
            projectRequest.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        if (request.AttachmentUrl != null)
            projectRequest.AttachmentUrl = string.IsNullOrWhiteSpace(request.AttachmentUrl) ? null : request.AttachmentUrl.Trim();

        if (request.AttachmentFileName != null)
            projectRequest.AttachmentFileName = string.IsNullOrWhiteSpace(request.AttachmentFileName) ? null : request.AttachmentFileName.Trim();

        if (request.Attachments != null || request.AttachmentUrl != null || request.AttachmentFileName != null)
            projectRequest.AttachmentsJson = ProjectRequestSerialization.SerializeAttachments(
                request.Attachments, projectRequest.AttachmentUrl, projectRequest.AttachmentFileName);

        if (request.AdminNotes != null)
            projectRequest.AdminNotes = request.AdminNotes;

        if (!string.IsNullOrWhiteSpace(request.RejectionReason))
            projectRequest.RejectionReason = request.RejectionReason.Trim();

        // --- Assignment ---
        var previousAssignee = projectRequest.AssignedToName;

        if (request.AssignedToUserId != null)
            projectRequest.AssignedToUserId = string.IsNullOrWhiteSpace(request.AssignedToUserId) ? null : request.AssignedToUserId.Trim();

        if (request.AssignedToName != null)
        {
            projectRequest.AssignedToName = string.IsNullOrWhiteSpace(request.AssignedToName) ? null : request.AssignedToName.Trim();
            if (projectRequest.AssignedToName != previousAssignee)
            {
                var desc = string.IsNullOrWhiteSpace(projectRequest.AssignedToName)
                    ? "Request unassigned"
                    : $"Assigned to {projectRequest.AssignedToName}";
                activities.Add(new ProjectRequestActivity
                {
                    Id = Guid.NewGuid(),
                    ProjectRequestId = projectRequest.Id,
                    ActionType = "Assigned",
                    Description = desc,
                    CreatedBy = request.UpdatedBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // --- Internal engineering fields ---
        if (request.Priority != null)
            projectRequest.Priority = string.IsNullOrWhiteSpace(request.Priority) ? null : request.Priority.Trim();

        if (request.TechnicalFeasibility != null)
            projectRequest.TechnicalFeasibility = string.IsNullOrWhiteSpace(request.TechnicalFeasibility) ? null : request.TechnicalFeasibility.Trim();

        if (request.EstimatedCost.HasValue)
            projectRequest.EstimatedCost = request.EstimatedCost.Value;

        if (request.EstimatedTimeline != null)
            projectRequest.EstimatedTimeline = string.IsNullOrWhiteSpace(request.EstimatedTimeline) ? null : request.EstimatedTimeline.Trim();

        if (request.ComplexityLevel != null)
            projectRequest.ComplexityLevel = string.IsNullOrWhiteSpace(request.ComplexityLevel) ? null : request.ComplexityLevel.Trim();

        if (request.InternalNotes != null)
        {
            var previous = projectRequest.InternalNotes;
            projectRequest.InternalNotes = request.InternalNotes;
            if (!string.IsNullOrWhiteSpace(request.InternalNotes) && request.InternalNotes != previous)
            {
                activities.Add(new ProjectRequestActivity
                {
                    Id = Guid.NewGuid(),
                    ProjectRequestId = projectRequest.Id,
                    ActionType = "NoteAdded",
                    Description = "Internal notes updated",
                    CreatedBy = request.UpdatedBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        if (activities.Count > 0)
            _context.Set<ProjectRequestActivity>().AddRange(activities);

        await _context.SaveChangesAsync(cancellationToken);

        // Send customer email on meaningful workflow status changes
        var customerEmail = string.IsNullOrWhiteSpace(projectRequest.Email)
            ? null : projectRequest.Email;
        if (!string.IsNullOrWhiteSpace(request.WorkflowStatus) &&
            request.WorkflowStatus != previousWorkflowStatus &&
            !string.IsNullOrWhiteSpace(customerEmail))
        {
            var baseUrl = _configuration["App:FrontendBaseUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
            var viewUrl = $"{baseUrl}/account/requests?requestId={projectRequest.Id}&type=project";
            await _emailService.SendRequestStatusUpdateAsync(
                customerEmail,
                projectRequest.FullName,
                projectRequest.ReferenceNumber ?? projectRequest.Id.ToString(),
                "Project Request",
                request.WorkflowStatus,
                viewUrl,
                request.RejectionReason,
                cancellationToken);
        }

        return true;
    }

    private static void ValidateTransition(ProjectRequest pr, string newStatus)
    {
        var current = pr.WorkflowStatus ?? ProjectRequestWorkflowStatus.New;

        switch (newStatus)
        {
            case ProjectRequestWorkflowStatus.QuoteSent:
                if (!pr.EstimatedCost.HasValue || pr.EstimatedCost <= 0)
                    throw new InvalidOperationException(
                        "Cannot send quote without an estimated cost. Fill in Estimated Cost first.");
                break;

            case ProjectRequestWorkflowStatus.Approved:
                if (current != ProjectRequestWorkflowStatus.QuoteSent)
                    throw new InvalidOperationException(
                        "Request can only be approved after a quote has been sent.");
                break;

            case ProjectRequestWorkflowStatus.InExecution:
                if (current != ProjectRequestWorkflowStatus.Approved)
                    throw new InvalidOperationException(
                        "Request can only move to In Execution after being approved.");
                break;

            case ProjectRequestWorkflowStatus.Completed:
                if (current != ProjectRequestWorkflowStatus.InExecution)
                    throw new InvalidOperationException(
                        "Request can only be completed when it is In Execution.");
                break;
        }
    }
}
