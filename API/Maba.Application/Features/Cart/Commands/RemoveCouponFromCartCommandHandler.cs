using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cart.DTOs;
using Maba.Application.Features.Cart.Queries;

namespace Maba.Application.Features.Cart.Commands;

public class RemoveCouponFromCartCommandHandler : IRequestHandler<RemoveCouponFromCartCommand, CartDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;

    public RemoveCouponFromCartCommandHandler(IApplicationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<CartDto> Handle(RemoveCouponFromCartCommand request, CancellationToken cancellationToken)
    {
        var cartQuery = _context.Set<Domain.Orders.Cart>().AsQueryable();

        Domain.Orders.Cart? cart = null;

        if (request.UserId.HasValue)
        {
            cart = await cartQuery.FirstOrDefaultAsync(c => c.UserId == request.UserId.Value, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(request.SessionId))
        {
            cart = await cartQuery.FirstOrDefaultAsync(c => c.SessionId == request.SessionId, cancellationToken);
        }

        if (cart == null)
        {
            throw new ArgumentException("Cart not found.");
        }

        cart.CouponCode = null;
        cart.CouponDiscount = 0;
        cart.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await _mediator.Send(new GetCartQuery
        {
            UserId = request.UserId,
            SessionId = request.SessionId
        }, cancellationToken) ?? new CartDto();
    }
}
