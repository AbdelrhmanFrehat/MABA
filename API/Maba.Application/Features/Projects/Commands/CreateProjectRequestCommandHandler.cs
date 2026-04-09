using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Projects.DTOs;
using Maba.Domain.Projects;

namespace Maba.Application.Features.Projects.Commands;

public class CreateProjectRequestCommandHandler : IRequestHandler<CreateProjectRequestCommand, ProjectRequestDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ICustomerResolverService _customerResolver;
    private readonly ILogger<CreateProjectRequestCommandHandler> _logger;

    public CreateProjectRequestCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        ICustomerResolverService customerResolver,
        ILogger<CreateProjectRequestCommandHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _customerResolver = customerResolver;
        _logger = logger;
    }

    public async Task<ProjectRequestDto> Handle(CreateProjectRequestCommand request, CancellationToken cancellationToken)
    {
        var customerId = await _customerResolver.ResolveAsync(
            request.UserId,
            request.FullName,
            request.Email,
            request.Phone,
            cancellationToken);

        var referenceNumber = await GenerateReferenceNumber(cancellationToken);

        var projectRequest = new ProjectRequest
        {
            Id = Guid.NewGuid(),
            ReferenceNumber = referenceNumber,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            RequestType = request.RequestType,
            ProjectId = request.ProjectId,
            ProjectType = request.ProjectType,
            MainDomain = request.MainDomain,
            RequiredCapabilitiesJson = ProjectRequestSerialization.SerializeCapabilities(request.RequiredCapabilities),
            Category = request.Category,
            BudgetRange = request.BudgetRange,
            Timeline = request.Timeline,
            ProjectStage = request.ProjectStage,
            Description = request.Description,
            AttachmentUrl = request.AttachmentUrl,
            AttachmentFileName = request.AttachmentFileName,
            AttachmentsJson = ProjectRequestSerialization.SerializeAttachments(request.Attachments, request.AttachmentUrl, request.AttachmentFileName),
            UserId = request.UserId,
            CustomerId = customerId,
            Status = ProjectRequestStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<ProjectRequest>().Add(projectRequest);

        var creationActivity = new ProjectRequestActivity
        {
            Id = Guid.NewGuid(),
            ProjectRequestId = projectRequest.Id,
            ActionType = "Created",
            Description = $"Request {referenceNumber} submitted by {projectRequest.FullName}",
            CreatedBy = projectRequest.Email,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<ProjectRequestActivity>().Add(creationActivity);

        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            await _emailService.SendRequestConfirmationAsync(projectRequest.Email, projectRequest.FullName, referenceNumber, "Project Request", null, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email for Project request {ReferenceNumber}", referenceNumber);
        }

        string? projectTitle = null;
        if (request.ProjectId.HasValue)
        {
            var project = await _context.Set<Project>()
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId.Value, cancellationToken);
            projectTitle = project?.TitleEn;
        }

        return new ProjectRequestDto
        {
            Id = projectRequest.Id,
            ReferenceNumber = projectRequest.ReferenceNumber,
            FullName = projectRequest.FullName,
            Email = projectRequest.Email,
            Phone = projectRequest.Phone,
            RequestType = projectRequest.RequestType,
            ProjectId = projectRequest.ProjectId,
            ProjectTitle = projectTitle,
            ProjectType = projectRequest.ProjectType,
            MainDomain = projectRequest.MainDomain,
            RequiredCapabilities = ProjectRequestSerialization.DeserializeCapabilities(projectRequest),
            Category = projectRequest.Category,
            BudgetRange = projectRequest.BudgetRange,
            Timeline = projectRequest.Timeline,
            Description = projectRequest.Description,
            ProjectDescription = projectRequest.Description,
            ProjectStage = projectRequest.ProjectStage,
            AttachmentUrl = projectRequest.AttachmentUrl,
            AttachmentFileName = projectRequest.AttachmentFileName,
            Attachments = ProjectRequestSerialization.DeserializeAttachments(projectRequest),
            Status = projectRequest.Status,
            WorkflowStatus = projectRequest.WorkflowStatus,
            CreatedAt = projectRequest.CreatedAt
        };
    }

    private async Task<string> GenerateReferenceNumber(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var prefix = $"PRJ-{today:yyyyMMdd}";
        
        var todayStart = today.Date;
        var todayEnd = todayStart.AddDays(1);
        
        var count = await _context.Set<ProjectRequest>()
            .CountAsync(r => r.CreatedAt >= todayStart && r.CreatedAt < todayEnd, cancellationToken);
        
        return $"{prefix}-{(count + 1):D3}";
    }
}
