using MediatR;
using Maba.Application.Features.Catalog.Items.DTOs;

namespace Maba.Application.Features.Catalog.Items.Commands;

public class ApproveCommentCommand : IRequest<CommentDto>
{
    public Guid CommentId { get; set; }
}

