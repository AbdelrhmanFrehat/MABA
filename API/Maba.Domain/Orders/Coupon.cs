using Maba.Domain.Common;

namespace Maba.Domain.Orders;

public enum CouponType
{
    Percentage = 0,
    FixedAmount = 1,
    FreeShipping = 2
}

public class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public CouponType Type { get; set; } = CouponType.Percentage;
    public decimal Value { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; }
    public int? UsagePerCustomer { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? CategoryId { get; set; }
    public bool AppliesToAllCategories { get; set; } = true;
}
