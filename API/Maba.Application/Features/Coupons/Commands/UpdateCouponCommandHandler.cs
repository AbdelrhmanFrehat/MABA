using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Coupons.Commands;

public class UpdateCouponCommandHandler : IRequestHandler<UpdateCouponCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateCouponCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _context.Set<Coupon>()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (coupon == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var codeUpper = request.Code.ToUpper().Trim();
            var exists = await _context.Set<Coupon>()
                .AnyAsync(c => c.Code.ToUpper() == codeUpper && c.Id != request.Id, cancellationToken);
            
            if (exists)
            {
                throw new ArgumentException("A coupon with this code already exists.");
            }
            
            coupon.Code = codeUpper;
        }

        if (request.DescriptionEn != null) coupon.DescriptionEn = request.DescriptionEn;
        if (request.DescriptionAr != null) coupon.DescriptionAr = request.DescriptionAr;
        if (request.Type.HasValue) coupon.Type = request.Type.Value;
        if (request.Value.HasValue) coupon.Value = request.Value.Value;
        if (request.MinOrderAmount.HasValue) coupon.MinOrderAmount = request.MinOrderAmount;
        if (request.MaxDiscountAmount.HasValue) coupon.MaxDiscountAmount = request.MaxDiscountAmount;
        if (request.UsageLimit.HasValue) coupon.UsageLimit = request.UsageLimit;
        if (request.UsagePerCustomer.HasValue) coupon.UsagePerCustomer = request.UsagePerCustomer;
        if (request.StartDate.HasValue) coupon.StartDate = request.StartDate;
        if (request.EndDate.HasValue) coupon.EndDate = request.EndDate;
        if (request.IsActive.HasValue) coupon.IsActive = request.IsActive.Value;

        coupon.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
