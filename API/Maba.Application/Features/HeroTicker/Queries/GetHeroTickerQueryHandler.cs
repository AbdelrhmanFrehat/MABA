using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.HeroTicker.DTOs;
using Maba.Domain.HeroTicker;

namespace Maba.Application.Features.HeroTicker.Queries;

public class GetHeroTickerQueryHandler : IRequestHandler<GetHeroTickerQuery, List<HeroTickerPublicDto>>
{
    private readonly IApplicationDbContext _context;

    public GetHeroTickerQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<HeroTickerPublicDto>> Handle(GetHeroTickerQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.Set<HeroTickerItem>()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        return items.Select(x => new HeroTickerPublicDto
        {
            Id = x.Id,
            Title = x.Title,
            ImageUrl = x.ImageUrl,
            SortOrder = x.SortOrder
        }).ToList();
    }
}
