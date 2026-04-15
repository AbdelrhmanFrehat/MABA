using MediatR;
using Maba.Application.Features.Projects.DTOs;
using Maba.Domain.Projects;

namespace Maba.Application.Features.Projects.Commands;

public class UpdateProjectRequestCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public ProjectRequestStatus? Status { get; set; }
    public string? WorkflowStatus { get; set; }
    public string? AdminNotes { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public ProjectRequestType? RequestType { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectType { get; set; }
    public string? MainDomain { get; set; }
    public List<string>? RequiredCapabilities { get; set; }
    public ProjectCategory? Category { get; set; }
    public string? BudgetRange { get; set; }
    public string? Timeline { get; set; }
    public string? ProjectStage { get; set; }
    public string? Description { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentFileName { get; set; }
    public List<ProjectRequestAttachmentDto>? Attachments { get; set; }
    // Assignment
    public string? AssignedToUserId { get; set; }
    public string? AssignedToName { get; set; }
    // Internal engineering fields
    public string? Priority { get; set; }
    public string? TechnicalFeasibility { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string? EstimatedTimeline { get; set; }
    public string? ComplexityLevel { get; set; }
    public string? InternalNotes { get; set; }
    public string? RejectionReason { get; set; }
    public string? UpdatedBy { get; set; }
}
