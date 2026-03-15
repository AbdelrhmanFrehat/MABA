using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Laser.DTOs;
using Maba.Domain.Laser;

namespace Maba.Application.Features.Laser.Requests.Commands;

public class UpdateLaserServiceRequestCommandHandler : IRequestHandler<UpdateLaserServiceRequestCommand, LaserServiceRequestDto?>
{
    private readonly IApplicationDbContext _context;

    public UpdateLaserServiceRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
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

        if (request.Status.HasValue)
        {
            var oldStatus = serviceRequest.Status;
            serviceRequest.Status = request.Status.Value;

            if (oldStatus == LaserServiceRequestStatus.Pending && 
                request.Status.Value != LaserServiceRequestStatus.Pending)
            {
                serviceRequest.ReviewedAt = DateTime.UtcNow;
            }

            if (request.Status.Value == LaserServiceRequestStatus.Completed)
            {
                serviceRequest.CompletedAt = DateTime.UtcNow;
            }
        }

        if (request.AdminNotes != null)
        {
            serviceRequest.AdminNotes = request.AdminNotes.Trim();
        }

        if (request.QuotedPrice.HasValue)
        {
            serviceRequest.QuotedPrice = request.QuotedPrice.Value;
        }

        serviceRequest.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

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
