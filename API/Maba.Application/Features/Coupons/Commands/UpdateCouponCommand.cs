using MediatR;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Coupons.Commands;

public class UpdateCouponCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public CouponType? Type { get; set; }
    public decimal? Value { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int? UsagePerCustomer { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
}
