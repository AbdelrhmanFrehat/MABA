using MediatR;
using Maba.Application.Features.Catalog.Items.DTOs;

namespace Maba.Application.Features.Catalog.Items.Commands;

public class CreateCommentCommand : IRequest<CommentDto>
{
    public Guid ItemId { get; set; }
    public Guid UserId { get; set; }
    public string Body { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
}

