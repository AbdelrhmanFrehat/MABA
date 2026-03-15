using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cart.DTOs;
using Maba.Application.Features.Cart.Queries;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Cart.Commands;

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, CartDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;

    public UpdateCartItemCommandHandler(IApplicationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<CartDto> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var cartItem = await _context.Set<CartItem>()
            .Include(ci => ci.Cart)
            .FirstOrDefaultAsync(ci => ci.Id == request.CartItemId, cancellationToken);

        if (cartItem == null)
        {
            throw new KeyNotFoundException("Cart item not found");
        }

        // Verify ownership
        if (request.UserId.HasValue && cartItem.Cart.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("Cannot update cart item");
        }
        if (!string.IsNullOrEmpty(request.SessionId) && cartItem.Cart.SessionId != request.SessionId)
        {
            throw new UnauthorizedAccessException("Cannot update cart item");
        }

        if (request.Quantity <= 0)
        {
            _context.Set<CartItem>().Remove(cartItem);
        }
        else
        {
            cartItem.Quantity = request.Quantity;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var query = new GetCartQuery { UserId = request.UserId, SessionId = request.SessionId };
        return (await _mediator.Send(query, cancellationToken))!;
    }
}
