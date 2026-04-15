using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Maba.Application.Common.ControlCenterJobs;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Laser.DTOs;
using Maba.Domain.Laser;
using Maba.Domain.ControlCenter;

namespace Maba.Application.Features.Laser.Requests.Commands;

public class UpdateLaserServiceRequestCommandHandler : IRequestHandler<UpdateLaserServiceRequestCommand, LaserServiceRequestDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly IControlCenterJobBridgeService _jobBridgeService;

    public UpdateLaserServiceRequestCommandHandler(
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

    public async Task<LaserServiceRequestDto?> Handle(UpdateLaserServiceRequestCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.Set<LaserServiceRequest>()
            .Include(r => r.Material)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (serviceRequest == null)
        {
            return null;
        }

        if (request.Status.HasValue &&
            request.Status.Value == LaserServiceRequestStatus.Rejected &&
            string.IsNullOrWhiteSpace(request.RejectionReason))
        {
            throw new InvalidOperationException("A rejection reason is required when rejecting a request.");
        }

        var previousStatus = serviceRequest.Status;

        if (request.Status.HasValue)
        {
            serviceRequest.Status = request.Status.Value;

            if (previousStatus == LaserServiceRequestStatus.Pending &&
                request.Status.Value != LaserServiceRequestStatus.Pending)
            {
                serviceRequest.ReviewedAt = DateTime.UtcNow;
            }

            if (request.Status.Value == LaserServiceRequestStatus.Completed)
                serviceRequest.CompletedAt = DateTime.UtcNow;
        }

        if (request.AdminNotes != null)
            serviceRequest.AdminNotes = request.AdminNotes.Trim();

        if (!string.IsNullOrWhiteSpace(request.RejectionReason))
            serviceRequest.RejectionReason = request.RejectionReason.Trim();

        if (request.QuotedPrice.HasValue)
            serviceRequest.QuotedPrice = request.QuotedPrice.Value;

        serviceRequest.UpdatedAt = DateTime.UtcNow;
        await SyncControlCenterJobAsync(serviceRequest, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        if (request.Status.HasValue && request.Status.Value != previousStatus)
        {
            var baseUrl = _configuration["App:FrontendBaseUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
            var viewUrl = $"{baseUrl}/account/requests?requestId={serviceRequest.Id}&type=laser";

            if (request.Status.Value == LaserServiceRequestStatus.Cancelled)
                await _emailService.SendRequestCancelledAsync(
                    serviceRequest.CustomerEmail, serviceRequest.CustomerName,
                    serviceRequest.ReferenceNumber, "Laser Request",
                    viewUrl, request.AdminNotes, cancellationToken);
            else
                await _emailService.SendRequestStatusUpdateAsync(
                    serviceRequest.CustomerEmail, serviceRequest.CustomerName,
                    serviceRequest.ReferenceNumber, "Laser Request",
                    request.Status.Value.ToString(), viewUrl,
                    request.RejectionReason, cancellationToken);
        }

        return new LaserServiceRequestDto
        {
            Id = serviceRequest.Id,
            ReferenceNumber = serviceRequest.ReferenceNumber,
            MaterialId = serviceRequest.MaterialId,
            MaterialNameEn = serviceRequest.Material?.NameEn ?? "",
            MaterialNameAr = serviceRequest.Material?.NameAr ?? "",
            OperationMode = serviceRequest.OperationMode,
            ImagePath = serviceRequest.ImagePath,
            ImageFileName = serviceRequest.ImageFileName,
            CustomerName = serviceRequest.CustomerName,
            CustomerEmail = serviceRequest.CustomerEmail,
            CustomerPhone = serviceRequest.CustomerPhone,
            CustomerNotes = serviceRequest.CustomerNotes,
            AdminNotes = serviceRequest.AdminNotes,
            QuotedPrice = serviceRequest.QuotedPrice,
            Status = serviceRequest.Status,
            CreatedAt = serviceRequest.CreatedAt,
            UpdatedAt = serviceRequest.UpdatedAt,
            ReviewedAt = serviceRequest.ReviewedAt,
            CompletedAt = serviceRequest.CompletedAt
        };
    }

    private async Task SyncControlCenterJobAsync(LaserServiceRequest serviceRequest, CancellationToken cancellationToken)
    {
        var (jobStatus, createIfMissing) = serviceRequest.Status switch
        {
            LaserServiceRequestStatus.Approved => (CcJobStatus.Ready, true),
            LaserServiceRequestStatus.InProgress => (CcJobStatus.InProgress, true),
            LaserServiceRequestStatus.Completed => (CcJobStatus.Completed, true),
            LaserServiceRequestStatus.Cancelled or LaserServiceRequestStatus.Rejected => (CcJobStatus.Cancelled, false),
            _ => (CcJobStatus.Pending, false)
        };

        await _jobBridgeService.EnsureJobAsync(new ControlCenterJobBridgeDefinition
        {
            SourceType = "LASER_REQUEST",
            SourceId = serviceRequest.Id,
            SourceReference = serviceRequest.ReferenceNumber,
            Title = serviceRequest.OperationMode == "cut"
                ? "Laser cutting job"
                : "Laser engraving job",
            Description = serviceRequest.CustomerNotes ?? serviceRequest.AdminNotes,
            CustomerName = serviceRequest.CustomerName,
            MachineType = "LASER",
            Status = jobStatus,
            Attachments = string.IsNullOrWhiteSpace(serviceRequest.ImageFileName)
                ? Array.Empty<ControlCenterJobFileReference>()
                : new[]
                {
                    new ControlCenterJobFileReference
                    {
                        FileName = serviceRequest.ImageFileName,
                        FilePath = serviceRequest.ImagePath,
                        Kind = "artwork"
                    }
                },
            PayloadJson = JsonSerializer.Serialize(new
            {
                serviceRequest.OperationMode,
                Material = serviceRequest.Material?.NameEn,
                serviceRequest.WidthCm,
                serviceRequest.HeightCm,
                serviceRequest.QuotedPrice,
                serviceRequest.CustomerNotes
            }),
            CreateIfMissing = createIfMissing
        }, cancellationToken);
    }
}
