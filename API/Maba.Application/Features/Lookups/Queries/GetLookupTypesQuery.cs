using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Lookups.DTOs;
using Maba.Domain.Lookups;

namespace Maba.Application.Features.Lookups.Queries;

public class GetLookupTypesQuery : IRequest<List<LookupTypeDto>>
{
}

public class GetLookupTypesQueryHandler : IRequestHandler<GetLookupTypesQuery, List<LookupTypeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLookupTypesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LookupTypeDto>> Handle(GetLookupTypesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Set<LookupType>()
            .OrderBy(x => x.Key)
            .Select(x => new LookupTypeDto
            {
                Id = x.Id,
                Key = x.Key,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                Description = x.Description,
                IsSystem = x.IsSystem,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
