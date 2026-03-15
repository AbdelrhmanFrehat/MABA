using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cart.DTOs;
using Maba.Application.Features.Cart.Queries;
using Maba.Domain.Orders;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Cart.Commands;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, CartDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;

    public AddToCartCommandHandler(IApplicationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<CartDto> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        // Validate item exists
        var item = await _context.Set<Item>()
            .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);

        if (item == null)
        {
            throw new KeyNotFoundException($"Item {request.ItemId} not found");
        }

        // Get or create cart
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

        bool isNewCart = false;
        if (cart == null)
        {
            isNewCart = true;
            cart = new Domain.Orders.Cart
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                SessionId = request.SessionId,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };
            _context.Set<Domain.Orders.Cart>().Add(cart);
            // Save the new cart first to ensure it exists in the database
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Check if item already in cart
        var existingItem = cart.Items.FirstOrDefault(ci => ci.ItemId == request.ItemId);
        
        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
            existingItem.UnitPrice = item.DiscountPrice ?? item.Price;
        }
        else
        {
            var cartItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ItemId = request.ItemId,
                Quantity = request.Quantity,
                UnitPrice = item.DiscountPrice ?? item.Price
            };
            // Add directly to the DbSet rather than the navigation property
            _context.Set<CartItem>().Add(cartItem);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Return updated cart
        var query = new GetCartQuery { UserId = request.UserId, SessionId = request.SessionId };
        return (await _mediator.Send(query, cancellationToken))!;
    }
}
