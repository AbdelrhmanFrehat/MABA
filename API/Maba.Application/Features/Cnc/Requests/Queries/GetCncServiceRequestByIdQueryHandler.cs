using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cnc.DTOs;
using Maba.Domain.Cnc;

namespace Maba.Application.Features.Cnc.Requests.Queries;

public class GetCncServiceRequestByIdQueryHandler : IRequestHandler<GetCncServiceRequestByIdQuery, CncServiceRequestDto?>
{
    private readonly IApplicationDbContext _context;

    public GetCncServiceRequestByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CncServiceRequestDto?> Handle(GetCncServiceRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.Set<CncServiceRequest>()
            .Include(x => x.Material)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        return serviceRequest == null ? null : CncServiceRequestDto.FromEntity(serviceRequest);
    }
}
