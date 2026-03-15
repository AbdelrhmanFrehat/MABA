using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.HeroTicker.DTOs;
using Maba.Domain.HeroTicker;

namespace Maba.Application.Features.HeroTicker.Queries;

public class GetHeroTickerAdminQueryHandler : IRequestHandler<GetHeroTickerAdminQuery, List<HeroTickerItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetHeroTickerAdminQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<HeroTickerItemDto>> Handle(GetHeroTickerAdminQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.Set<HeroTickerItem>()
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        return items.Select(Map).ToList();
    }

    private static HeroTickerItemDto Map(HeroTickerItem x) => new()
    {
        Id = x.Id,
        Title = x.Title,
        ImageUrl = x.ImageUrl,
        SortOrder = x.SortOrder,
        IsActive = x.IsActive,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt
    };
}
