namespace Maba.Application.Features.Catalog.Items.DTOs;

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

