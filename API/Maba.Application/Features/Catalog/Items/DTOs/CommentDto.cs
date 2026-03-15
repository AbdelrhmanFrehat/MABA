namespace Maba.Application.Features.Catalog.Items.DTOs;

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
    public bool IsApproved { get; set; }
    public List<CommentDto> Replies { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

