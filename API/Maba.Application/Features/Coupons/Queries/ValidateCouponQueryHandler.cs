using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Coupons.DTOs;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Coupons.Queries;

public class ValidateCouponQueryHandler : IRequestHandler<ValidateCouponQuery, ValidateCouponResultDto>
{
    private readonly IApplicationDbContext _context;

    public ValidateCouponQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ValidateCouponResultDto> Handle(ValidateCouponQuery request, CancellationToken cancellationToken)
    {
        var coupon = await _context.Set<Coupon>()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == request.Code.ToUpper().Trim(), cancellationToken);

        if (coupon == null)
        {
            return new ValidateCouponResultDto
            {
                IsValid = false,
                ErrorMessage = "Invalid coupon code."
            };
        }

        if (!coupon.IsActive)
        {
            return new ValidateCouponResultDto
            {
                IsValid = false,
                ErrorMessage = "This coupon is no longer active."
            };
        }

        if (coupon.StartDate.HasValue && coupon.StartDate.Value > DateTime.UtcNow)
        {
            return new ValidateCouponResultDto
            {
                IsValid = false,
                ErrorMessage = "This coupon is not yet valid."
            };
        }

        if (coupon.EndDate.HasValue && coupon.EndDate.Value < DateTime.UtcNow)
        {
            return new ValidateCouponResultDto
            {
                IsValid = false,
                ErrorMessage = "This coupon has expired."
            };
        }

        if (coupon.UsageLimit.HasValue && coupon.UsageCount >= coupon.UsageLimit.Value)
        {
            return new ValidateCouponResultDto
            {
                IsValid = false,
                ErrorMessage = "This coupon has reached its usage limit."
            };
        }

        if (coupon.MinOrderAmount.HasValue && request.OrderTotal < coupon.MinOrderAmount.Value)
        {
            return new ValidateCouponResultDto
            {
                IsValid = false,
                ErrorMessage = $"Minimum order amount for this coupon is {coupon.MinOrderAmount.Value:C}."
            };
        }

        decimal discountAmount = 0;
        switch (coupon.Type)
        {
            case CouponType.Percentage:
                discountAmount = request.OrderTotal * (coupon.Value / 100);
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

        if (discountAmount > request.OrderTotal)
        {
            discountAmount = request.OrderTotal;
        }

        return new ValidateCouponResultDto
        {
            IsValid = true,
            DiscountAmount = discountAmount,
            Coupon = new CouponDto
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
            }
        };
    }
}
