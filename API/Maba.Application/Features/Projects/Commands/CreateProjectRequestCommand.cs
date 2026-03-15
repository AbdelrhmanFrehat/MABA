using MediatR;
using Maba.Application.Features.Projects.DTOs;
using Maba.Domain.Projects;

namespace Maba.Application.Features.Projects.Commands;

public class CreateProjectRequestCommand : IRequest<ProjectRequestDto>
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public ProjectRequestType RequestType { get; set; }
    public Guid? ProjectId { get; set; }
    public ProjectCategory? Category { get; set; }
    public string? BudgetRange { get; set; }
    public string? Timeline { get; set; }
    public string? Description { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentFileName { get; set; }
}
