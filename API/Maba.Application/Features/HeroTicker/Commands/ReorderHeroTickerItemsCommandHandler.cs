using MediatR;
using Maba.Application.Common.Interfaces;
using Maba.Domain.HeroTicker;
using Microsoft.EntityFrameworkCore;

namespace Maba.Application.Features.HeroTicker.Commands;

public class ReorderHeroTickerItemsCommandHandler : IRequestHandler<ReorderHeroTickerItemsCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ReorderHeroTickerItemsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ReorderHeroTickerItemsCommand request, CancellationToken cancellationToken)
    {
        var ids = request.Items.Select(x => x.Id).ToList();
        var items = await _context.Set<HeroTickerItem>()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);

        foreach (var orderItem in request.Items)
        {
            var item = items.FirstOrDefault(x => x.Id == orderItem.Id);
            if (item != null)
                item.SortOrder = orderItem.SortOrder;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
