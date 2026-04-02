namespace Maba.Application.Features.Catalog.Items.DTOs;

public class AdminReviewListItemDto
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string ItemNameEn { get; set; } = string.Empty;
    public string ItemNameAr { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public int HelpfulCount { get; set; }
    public List<object> Replies { get; set; } = new();
}

public class AdminReviewListResponse
{
    public List<AdminReviewListItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public decimal AverageRating { get; set; }
}
