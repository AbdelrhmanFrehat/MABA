using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Cnc;

namespace Maba.Application.Features.Cnc.Requests.Commands;

public class DeleteCncServiceRequestCommandHandler : IRequestHandler<DeleteCncServiceRequestCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteCncServiceRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteCncServiceRequestCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.Set<CncServiceRequest>()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (serviceRequest == null)
        {
            return false;
        }

        _context.Set<CncServiceRequest>().Remove(serviceRequest);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
