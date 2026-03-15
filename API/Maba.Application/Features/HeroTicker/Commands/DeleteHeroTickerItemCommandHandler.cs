using MediatR;
using Maba.Application.Common.Interfaces;
using Maba.Domain.HeroTicker;

namespace Maba.Application.Features.HeroTicker.Commands;

public class DeleteHeroTickerItemCommandHandler : IRequestHandler<DeleteHeroTickerItemCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteHeroTickerItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteHeroTickerItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.Set<HeroTickerItem>().FindAsync(new object[] { request.Id }, cancellationToken);
        if (item == null) return false;

        _context.Set<HeroTickerItem>().Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
