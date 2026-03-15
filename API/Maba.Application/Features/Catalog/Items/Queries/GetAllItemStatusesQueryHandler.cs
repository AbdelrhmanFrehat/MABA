using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Application.Features.Catalog.Items.Queries;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Items.Handlers;

public class GetAllItemStatusesQueryHandler : IRequestHandler<GetAllItemStatusesQuery, List<ItemStatusDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllItemStatusesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ItemStatusDto>> Handle(GetAllItemStatusesQuery request, CancellationToken cancellationToken)
    {
        var statuses = await _context.Set<ItemStatus>()
            .OrderBy(s => s.NameEn)
            .ToListAsync(cancellationToken);

        return statuses.Select(s => new ItemStatusDto
        {
            Id = s.Id,
            Key = s.Key,
            NameEn = s.NameEn,
            NameAr = s.NameAr
        }).ToList();
    }
}
