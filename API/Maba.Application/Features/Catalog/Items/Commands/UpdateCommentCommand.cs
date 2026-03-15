using MediatR;
using Maba.Application.Features.Catalog.Items.DTOs;

namespace Maba.Application.Features.Catalog.Items.Commands;

public class UpdateCommentCommand : IRequest<CommentDto>
{
    public Guid CommentId { get; set; }
    public string Body { get; set; } = string.Empty;
}

