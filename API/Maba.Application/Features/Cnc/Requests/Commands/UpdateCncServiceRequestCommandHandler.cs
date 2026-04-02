using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cnc.DTOs;
using Maba.Domain.Cnc;

namespace Maba.Application.Features.Cnc.Requests.Commands;

public class UpdateCncServiceRequestCommandHandler : IRequestHandler<UpdateCncServiceRequestCommand, CncServiceRequestDto?>
{
    private readonly IApplicationDbContext _context;

    public UpdateCncServiceRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CncServiceRequestDto?> Handle(UpdateCncServiceRequestCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.Set<CncServiceRequest>()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (serviceRequest == null)
        {
            return null;
        }

        if (request.Status.HasValue)
        {
            var oldStatus = serviceRequest.Status;
            serviceRequest.Status = request.Status.Value;

            if (oldStatus == CncServiceRequestStatus.Pending &&
                (request.Status.Value == CncServiceRequestStatus.InReview ||
                 request.Status.Value == CncServiceRequestStatus.Rejected))
            {
                serviceRequest.ReviewedAt = DateTime.UtcNow;
            }

            if (request.Status.Value == CncServiceRequestStatus.Completed)
            {
                serviceRequest.CompletedAt = DateTime.UtcNow;
            }
        }

        if (request.AdminNotes is not null)
        {
            serviceRequest.AdminNotes = request.AdminNotes;
        }

        if (request.EstimatedPrice.HasValue)
        {
            serviceRequest.EstimatedPrice = request.EstimatedPrice.Value;
        }

        if (request.FinalPrice.HasValue)
        {
            serviceRequest.FinalPrice = request.FinalPrice.Value;
        }

        serviceRequest.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var updated = await _context.Set<CncServiceRequest>()
            .Include(x => x.Material)
            .FirstAsync(x => x.Id == request.Id, cancellationToken);

        return CncServiceRequestDto.FromEntity(updated);
    }
}
