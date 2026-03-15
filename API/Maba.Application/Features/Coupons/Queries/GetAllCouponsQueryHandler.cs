using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Coupons.DTOs;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Coupons.Queries;

public class GetAllCouponsQueryHandler : IRequestHandler<GetAllCouponsQuery, List<CouponDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllCouponsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CouponDto>> Handle(GetAllCouponsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Coupon>().AsQueryable();

        if (request.ActiveOnly == true)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CouponDto
            {
                Id = c.Id,
                Code = c.Code,
                DescriptionEn = c.DescriptionEn,
                DescriptionAr = c.DescriptionAr,
                Type = c.Type,
                Value = c.Value,
                MinOrderAmount = c.MinOrderAmount,
                MaxDiscountAmount = c.MaxDiscountAmount,
                UsageLimit = c.UsageLimit,
                UsageCount = c.UsageCount,
                UsagePerCustomer = c.UsagePerCustomer,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
