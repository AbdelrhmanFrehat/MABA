using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cart.DTOs;
using Maba.Domain.Orders;
using Maba.Domain.Media;

namespace Maba.Application.Features.Cart.Queries;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto?>
{
    private readonly IApplicationDbContext _context;

    public GetCartQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CartDto?> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        Domain.Orders.Cart? cart = null;

        if (request.UserId.HasValue)
        {
            cart = await _context.Set<Domain.Orders.Cart>()
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Item)
                .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(request.SessionId))
        {
            cart = await _context.Set<Domain.Orders.Cart>()
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Item)
                .FirstOrDefaultAsync(c => c.SessionId == request.SessionId, cancellationToken);
        }

        if (cart == null)
        {
            return null;
        }

        // Get primary images for items
        var itemIds = cart.Items.Select(ci => ci.ItemId).ToList();
        var primaryImages = await _context.Set<EntityMediaLink>()
            .Include(eml => eml.MediaAsset)
            .Where(eml => eml.EntityType == "Item" && itemIds.Contains(eml.EntityId) && eml.IsPrimary)
            .ToDictionaryAsync(eml => eml.EntityId, eml => eml.MediaAsset.FileUrl, cancellationToken);

        return MapToDto(cart, primaryImages);
    }

    private CartDto MapToDto(Domain.Orders.Cart cart, Dictionary<Guid, string> primaryImages)
    {
        var items = cart.Items.Select(ci => new CartItemDto
        {
            Id = ci.Id,
            CartId = ci.CartId,
            ItemId = ci.ItemId,
            ItemNameEn = ci.Item.NameEn,
            ItemNameAr = ci.Item.NameAr,
            ItemSku = ci.Item.Sku,
            Quantity = ci.Quantity,
            UnitPrice = ci.UnitPrice,
            Subtotal = ci.UnitPrice * ci.Quantity,
            MediaAssetUrl = primaryImages.GetValueOrDefault(ci.ItemId)
        }).ToList();

        var subtotal = items.Sum(i => i.Subtotal);
        var taxAmount = subtotal * 0.15m; // 15% VAT
        var shippingAmount = subtotal > 200 ? 0 : 25m; // Free shipping over 200
        var discountAmount = cart.CouponDiscount;
        var total = subtotal + taxAmount + shippingAmount - discountAmount;

        return new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            SessionId = cart.SessionId,
            Items = items,
            Subtotal = subtotal,
            TaxAmount = taxAmount,
            ShippingAmount = shippingAmount,
            DiscountAmount = discountAmount,
            Total = total,
            Currency = "ILS",
            CouponCode = cart.CouponCode,
            CouponDiscount = cart.CouponDiscount,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt
        };
    }
}
