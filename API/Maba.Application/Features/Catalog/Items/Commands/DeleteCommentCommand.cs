using MediatR;

namespace Maba.Application.Features.Catalog.Items.Commands;

public class DeleteCommentCommand : IRequest<Unit>
{
    public Guid CommentId { get; set; }
}

