using Maba.Domain.Orders;

namespace Maba.Application.Features.Coupons.DTOs;

public class CouponDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    
    public CouponType Type { get; set; }
    public string TypeName => Type.ToString();
    public decimal Value { get; set; }
    
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    
    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; }
    public int? UsagePerCustomer { get; set; }
    
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public bool IsActive { get; set; }
    public bool IsExpired => EndDate.HasValue && EndDate.Value < DateTime.UtcNow;
    public bool IsExhausted => UsageLimit.HasValue && UsageCount >= UsageLimit.Value;
    public bool IsValid => IsActive && !IsExpired && !IsExhausted;
    
    public DateTime CreatedAt { get; set; }
}

public class ValidateCouponResultDto
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal DiscountAmount { get; set; }
    public CouponDto? Coupon { get; set; }
}

public class CreateCouponRequest
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

public class UpdateCouponRequest : CreateCouponRequest
{
    public Guid Id { get; set; }
}
