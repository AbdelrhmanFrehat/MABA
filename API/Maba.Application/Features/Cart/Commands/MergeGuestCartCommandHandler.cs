using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cart.DTOs;
using Maba.Application.Features.Cart.Queries;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Cart.Commands;

public class MergeGuestCartCommandHandler : IRequestHandler<MergeGuestCartCommand, CartDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;

    public MergeGuestCartCommandHandler(IApplicationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<CartDto?> Handle(MergeGuestCartCommand request, CancellationToken cancellationToken)
    {
        // Get guest cart
        var guestCart = await _context.Set<Domain.Orders.Cart>()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.SessionId == request.SessionId, cancellationToken);

        if (guestCart == null || !guestCart.Items.Any())
        {
            // No guest cart, just return user's cart if exists
            return await _mediator.Send(new GetCartQuery { UserId = request.UserId }, cancellationToken);
        }

        // Get or create user cart
        var userCart = await _context.Set<Domain.Orders.Cart>()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

        if (userCart == null)
        {
            // Convert guest cart to user cart
            guestCart.UserId = request.UserId;
            guestCart.SessionId = null;
            await _context.SaveChangesAsync(cancellationToken);
            return await _mediator.Send(new GetCartQuery { UserId = request.UserId }, cancellationToken);
        }

        // Merge items from guest cart to user cart
        foreach (var guestItem in guestCart.Items)
        {
            var existingItem = userCart.Items.FirstOrDefault(i => i.ItemId == guestItem.ItemId);
            if (existingItem != null)
            {
                // Add quantities
                existingItem.Quantity += guestItem.Quantity;
            }
            else
            {
                // Move item to user cart
                var newItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = userCart.Id,
                    ItemId = guestItem.ItemId,
                    Quantity = guestItem.Quantity,
                    UnitPrice = guestItem.UnitPrice
                };
                userCart.Items.Add(newItem);
            }
        }

        // Delete guest cart
        _context.Set<CartItem>().RemoveRange(guestCart.Items);
        _context.Set<Domain.Orders.Cart>().Remove(guestCart);

        await _context.SaveChangesAsync(cancellationToken);

        return await _mediator.Send(new GetCartQuery { UserId = request.UserId }, cancellationToken);
    }
}
