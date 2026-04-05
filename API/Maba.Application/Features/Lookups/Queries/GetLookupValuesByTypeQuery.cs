using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Lookups.DTOs;
using Maba.Domain.Lookups;

namespace Maba.Application.Features.Lookups.Queries;

public class GetLookupValuesByTypeQuery : IRequest<List<LookupValueDto>>
{
    public string TypeKey { get; set; } = string.Empty;
}

public class GetLookupValuesByTypeQueryHandler : IRequestHandler<GetLookupValuesByTypeQuery, List<LookupValueDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLookupValuesByTypeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LookupValueDto>> Handle(GetLookupValuesByTypeQuery request, CancellationToken cancellationToken)
    {
        return await _context.Set<LookupValue>()
            .Where(x => x.LookupType.Key == request.TypeKey)
            .OrderBy(x => x.SortOrder)
            .Select(x => new LookupValueDto
            {
                Id = x.Id,
                LookupTypeId = x.LookupTypeId,
                Key = x.Key,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                Description = x.Description,
                SortOrder = x.SortOrder,
                IsDefault = x.IsDefault,
                IsSystem = x.IsSystem,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
