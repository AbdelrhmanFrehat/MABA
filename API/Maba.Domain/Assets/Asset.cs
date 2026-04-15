using Maba.Domain.Common;
using Maba.Domain.Lookups;
using Maba.Domain.Media;
using Maba.Domain.Orders;
using Maba.Domain.Users;

namespace Maba.Domain.Assets;

public class Asset : BaseEntity
{
    public string AssetNumber { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }

    public Guid AssetCategoryId { get; set; }
    public Guid InvestorUserId { get; set; }
    public Guid AcquisitionConditionId { get; set; }
    public string? ConditionNotes { get; set; }

    public decimal ApproximatePrice { get; set; }
    public string Currency { get; set; } = "ILS";
    public DateTime AcquiredAt { get; set; }

    public Guid StatusId { get; set; }
    public string? LocationNotes { get; set; }

    public Guid? PhotoMediaId { get; set; }

    // Navigation
    public AssetCategory AssetCategory { get; set; } = null!;
    public User InvestorUser { get; set; } = null!;
    public LookupValue AcquisitionCondition { get; set; } = null!;
    public LookupValue Status { get; set; } = null!;
    public MediaAsset? PhotoMedia { get; set; }
}
