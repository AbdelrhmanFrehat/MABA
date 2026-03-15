using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Cnc;

namespace Maba.Application.Features.Cnc.Requests.Commands;

public class UpdateCncServiceRequestCommandHandler : IRequestHandler<UpdateCncServiceRequestCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateCncServiceRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateCncServiceRequestCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.Set<CncServiceRequest>()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (serviceRequest == null)
        {
            return false;
        }

        if (request.Status.HasValue)
        {
            var oldStatus = serviceRequest.Status;
            serviceRequest.Status = request.Status.Value;

            if (oldStatus == CncServiceRequestStatus.Pending && 
                request.Status.Value == CncServiceRequestStatus.InReview)
            {
                serviceRequest.ReviewedAt = DateTime.UtcNow;
            }

            if (request.Status.Value == CncServiceRequestStatus.Completed)
            {
                serviceRequest.CompletedAt = DateTime.UtcNow;
            }
        }

        if (request.AdminNotes != null)
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
        return true;
    }
}
