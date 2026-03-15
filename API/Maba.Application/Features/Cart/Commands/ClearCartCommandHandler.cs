using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Cart.Commands;

public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public ClearCartCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        Domain.Orders.Cart? cart = null;

        if (request.UserId.HasValue)
        {
            cart = await _context.Set<Domain.Orders.Cart>()
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(request.SessionId))
        {
            cart = await _context.Set<Domain.Orders.Cart>()
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.SessionId == request.SessionId, cancellationToken);
        }

        if (cart != null)
        {
            _context.Set<CartItem>().RemoveRange(cart.Items);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
