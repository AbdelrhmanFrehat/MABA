using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cnc.DTOs;
using Maba.Domain.Cnc;

namespace Maba.Application.Features.Cnc.Requests.Commands;

public class UpdateCncServiceRequestCommandHandler : IRequestHandler<UpdateCncServiceRequestCommand, CncServiceRequestDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public UpdateCncServiceRequestCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
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
}
