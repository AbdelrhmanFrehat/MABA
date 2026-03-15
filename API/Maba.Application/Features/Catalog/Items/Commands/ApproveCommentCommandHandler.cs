using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Items.Commands;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Items.Handlers;

public class ApproveCommentCommandHandler : IRequestHandler<ApproveCommentCommand, CommentDto>
{
    private readonly IApplicationDbContext _context;

    public ApproveCommentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CommentDto> Handle(ApproveCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _context.Set<Comment>()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);

        if (comment == null)
        {
            throw new KeyNotFoundException("Comment not found.");
        }

        comment.IsApproved = true;
        comment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new CommentDto
        {
            Id = comment.Id,
            ItemId = comment.ItemId,
            UserId = comment.UserId,
            UserFullName = comment.User.FullName,
            Body = comment.Body,
            ParentCommentId = comment.ParentCommentId,
            IsApproved = comment.IsApproved,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }
}

