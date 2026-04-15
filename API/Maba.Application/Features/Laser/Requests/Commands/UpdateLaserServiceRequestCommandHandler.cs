using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Laser.DTOs;
using Maba.Domain.Laser;

namespace Maba.Application.Features.Laser.Requests.Commands;

public class UpdateLaserServiceRequestCommandHandler : IRequestHandler<UpdateLaserServiceRequestCommand, LaserServiceRequestDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public UpdateLaserServiceRequestCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
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
}
