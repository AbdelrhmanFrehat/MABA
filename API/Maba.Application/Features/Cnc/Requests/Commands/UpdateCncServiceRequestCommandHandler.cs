using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Maba.Application.Common.ControlCenterJobs;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cnc.DTOs;
using Maba.Domain.Cnc;
using Maba.Domain.ControlCenter;

namespace Maba.Application.Features.Cnc.Requests.Commands;

public class UpdateCncServiceRequestCommandHandler : IRequestHandler<UpdateCncServiceRequestCommand, CncServiceRequestDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly IControlCenterJobBridgeService _jobBridgeService;

    public UpdateCncServiceRequestCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        IConfiguration configuration,
        IControlCenterJobBridgeService jobBridgeService)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
        _jobBridgeService = jobBridgeService;
    }

    public async Task<CncServiceRequestDto?> Handle(UpdateCncServiceRequestCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.Set<CncServiceRequest>()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (serviceRequest == null) return null;

        if (request.Status.HasValue &&
            request.Status.Value == CncServiceRequestStatus.Rejected &&
            string.IsNullOrWhiteSpace(request.RejectionReason))
        {
            throw new InvalidOperationException("A rejection reason is required when rejecting a request.");
        }

        var previousStatus = serviceRequest.Status;

        if (request.Status.HasValue)
        {
            serviceRequest.Status = request.Status.Value;

            if (previousStatus == CncServiceRequestStatus.Pending &&
                (request.Status.Value == CncServiceRequestStatus.InReview ||
                 request.Status.Value == CncServiceRequestStatus.Rejected))
            {
                serviceRequest.ReviewedAt = DateTime.UtcNow;
            }

            if (request.Status.Value == CncServiceRequestStatus.Completed)
                serviceRequest.CompletedAt = DateTime.UtcNow;
        }

        if (request.AdminNotes is not null)
            serviceRequest.AdminNotes = request.AdminNotes;

        if (!string.IsNullOrWhiteSpace(request.RejectionReason))
            serviceRequest.RejectionReason = request.RejectionReason.Trim();

        if (request.EstimatedPrice.HasValue)
            serviceRequest.EstimatedPrice = request.EstimatedPrice.Value;

        if (request.FinalPrice.HasValue)
            serviceRequest.FinalPrice = request.FinalPrice.Value;

        serviceRequest.UpdatedAt = DateTime.UtcNow;
        await SyncControlCenterJobAsync(serviceRequest, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        // Send customer email if status changed
        if (request.Status.HasValue && request.Status.Value != previousStatus)
        {
            var baseUrl = _configuration["App:FrontendBaseUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
            var viewUrl = $"{baseUrl}/account/requests?requestId={serviceRequest.Id}&type=cnc";

            if (request.Status.Value == CncServiceRequestStatus.Cancelled)
                await _emailService.SendRequestCancelledAsync(
                    serviceRequest.CustomerEmail, serviceRequest.CustomerName,
                    serviceRequest.ReferenceNumber, "CNC Request",
                    viewUrl, request.AdminNotes, cancellationToken);
            else
                await _emailService.SendRequestStatusUpdateAsync(
                    serviceRequest.CustomerEmail, serviceRequest.CustomerName,
                    serviceRequest.ReferenceNumber, "CNC Request",
                    request.Status.Value.ToString(), viewUrl,
                    request.RejectionReason, cancellationToken);
        }

        var updated = await _context.Set<CncServiceRequest>()
            .Include(x => x.Material)
            .FirstAsync(x => x.Id == request.Id, cancellationToken);

        return CncServiceRequestDto.FromEntity(updated);
    }

    private async Task SyncControlCenterJobAsync(CncServiceRequest serviceRequest, CancellationToken cancellationToken)
    {
        var (jobStatus, createIfMissing) = serviceRequest.Status switch
        {
            CncServiceRequestStatus.Accepted => (CcJobStatus.Ready, true),
            CncServiceRequestStatus.InProgress => (CcJobStatus.InProgress, true),
            CncServiceRequestStatus.Completed => (CcJobStatus.Completed, true),
            CncServiceRequestStatus.Cancelled or CncServiceRequestStatus.Rejected => (CcJobStatus.Cancelled, false),
            _ => (CcJobStatus.Pending, false)
        };

        await _jobBridgeService.EnsureJobAsync(new ControlCenterJobBridgeDefinition
        {
            SourceType = "CNC_REQUEST",
            SourceId = serviceRequest.Id,
            SourceReference = serviceRequest.ReferenceNumber,
            Title = $"CNC {serviceRequest.ServiceMode} job",
            Description = serviceRequest.ProjectDescription ?? serviceRequest.DesignNotes ?? serviceRequest.AdminNotes,
            CustomerName = serviceRequest.CustomerName,
            MachineType = "CNC",
            Status = jobStatus,
            Priority = serviceRequest.OperationType,
            Attachments = string.IsNullOrWhiteSpace(serviceRequest.FileName)
                ? Array.Empty<ControlCenterJobFileReference>()
                : new[]
                {
                    new ControlCenterJobFileReference
                    {
                        FileName = serviceRequest.FileName!,
                        FilePath = serviceRequest.FilePath,
                        Kind = "source-file"
                    }
                },
            PayloadJson = JsonSerializer.Serialize(new
            {
                serviceRequest.ServiceMode,
                serviceRequest.OperationType,
                serviceRequest.PcbMaterial,
                serviceRequest.PcbThickness,
                serviceRequest.PcbSide,
                serviceRequest.PcbOperation,
                serviceRequest.WidthMm,
                serviceRequest.HeightMm,
                serviceRequest.ThicknessMm,
                serviceRequest.Quantity,
                serviceRequest.DepthMode,
                serviceRequest.DepthMm,
                serviceRequest.DesignSourceType,
                serviceRequest.DesignNotes,
                serviceRequest.FinalPrice,
                serviceRequest.EstimatedPrice
            }),
            CreateIfMissing = createIfMissing
        }, cancellationToken);
    }
}
