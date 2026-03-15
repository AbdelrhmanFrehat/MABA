using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cart.DTOs;
using Maba.Application.Features.Cart.Queries;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Cart.Commands;

public class ApplyCouponToCartCommandHandler : IRequestHandler<ApplyCouponToCartCommand, CartDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;

    public ApplyCouponToCartCommandHandler(IApplicationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<CartDto> Handle(ApplyCouponToCartCommand request, CancellationToken cancellationToken)
    {
        var cartQuery = _context.Set<Domain.Orders.Cart>()
            .Include(c => c.Items)
            .ThenInclude(ci => ci.Item)
            .AsQueryable();

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

        var coupon = await _context.Set<Coupon>()
            .FirstOrDefaultAsync(c => c.Code.ToLower() == request.CouponCode.ToLower() && c.IsActive, cancellationToken);

        if (coupon == null)
        {
            throw new ArgumentException("Invalid coupon code.");
        }

        if (coupon.StartDate.HasValue && DateTime.UtcNow < coupon.StartDate.Value)
        {
            throw new ArgumentException("This coupon is not yet active.");
        }

        if (coupon.EndDate.HasValue && DateTime.UtcNow > coupon.EndDate.Value)
        {
            throw new ArgumentException("This coupon has expired.");
        }

        if (coupon.UsageLimit.HasValue && coupon.UsageCount >= coupon.UsageLimit.Value)
        {
            throw new ArgumentException("This coupon has reached its usage limit.");
        }

        var subtotal = cart.Items.Sum(i => i.Quantity * i.UnitPrice);

        if (coupon.MinOrderAmount.HasValue && subtotal < coupon.MinOrderAmount.Value)
        {
            throw new ArgumentException($"Minimum order amount of {coupon.MinOrderAmount:C} is required for this coupon.");
        }

        decimal discountAmount = 0;
        switch (coupon.Type)
        {
            case CouponType.Percentage:
                discountAmount = subtotal * (coupon.Value / 100);
                break;
            case CouponType.FixedAmount:
                discountAmount = coupon.Value;
                break;
            case CouponType.FreeShipping:
                discountAmount = 0;
                break;
        }

        if (coupon.MaxDiscountAmount.HasValue && discountAmount > coupon.MaxDiscountAmount.Value)
        {
            discountAmount = coupon.MaxDiscountAmount.Value;
        }

        if (discountAmount > subtotal)
        {
            discountAmount = subtotal;
        }

        cart.CouponCode = coupon.Code;
        cart.CouponDiscount = discountAmount;
        cart.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await _mediator.Send(new GetCartQuery
        {
            UserId = request.UserId,
            SessionId = request.SessionId
        }, cancellationToken) ?? new CartDto();
    }
}
