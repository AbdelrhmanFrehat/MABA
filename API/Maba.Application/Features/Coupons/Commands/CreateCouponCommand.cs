using MediatR;
using Maba.Application.Features.Coupons.DTOs;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Coupons.Commands;

public class CreateCouponCommand : IRequest<CouponDto>
{
    public string Code { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public CouponType Type { get; set; } = CouponType.Percentage;
    public decimal Value { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int? UsagePerCustomer { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}
