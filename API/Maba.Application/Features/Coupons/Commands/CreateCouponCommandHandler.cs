using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Coupons.DTOs;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Coupons.Commands;

public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, CouponDto>
{
    private readonly IApplicationDbContext _context;

    public CreateCouponCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CouponDto> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        var codeUpper = request.Code.ToUpper().Trim();

        var exists = await _context.Set<Coupon>()
            .AnyAsync(c => c.Code.ToUpper() == codeUpper, cancellationToken);

        if (exists)
        {
            throw new ArgumentException("A coupon with this code already exists.");
        }

        var coupon = new Coupon
        {
            Id = Guid.NewGuid(),
            Code = codeUpper,
            DescriptionEn = request.DescriptionEn,
            DescriptionAr = request.DescriptionAr,
            Type = request.Type,
            Value = request.Value,
            MinOrderAmount = request.MinOrderAmount,
            MaxDiscountAmount = request.MaxDiscountAmount,
            UsageLimit = request.UsageLimit,
            UsagePerCustomer = request.UsagePerCustomer,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            UsageCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<Coupon>().Add(coupon);
        await _context.SaveChangesAsync(cancellationToken);

        return new CouponDto
        {
            Id = coupon.Id,
            Code = coupon.Code,
            DescriptionEn = coupon.DescriptionEn,
            DescriptionAr = coupon.DescriptionAr,
            Type = coupon.Type,
            Value = coupon.Value,
            MinOrderAmount = coupon.MinOrderAmount,
            MaxDiscountAmount = coupon.MaxDiscountAmount,
            UsageLimit = coupon.UsageLimit,
            UsageCount = coupon.UsageCount,
            UsagePerCustomer = coupon.UsagePerCustomer,
            StartDate = coupon.StartDate,
            EndDate = coupon.EndDate,
            IsActive = coupon.IsActive,
            CreatedAt = coupon.CreatedAt
        };
    }
}
