using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Items.Commands;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Items.Handlers;

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, CommentDto>
{
    private readonly IApplicationDbContext _context;

    public CreateCommentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CommentDto> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        // Check if item exists
        var item = await _context.Set<Item>()
            .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);

        if (item == null)
        {
            throw new KeyNotFoundException("Item not found.");
        }

        // If parent comment specified, verify it exists and belongs to same item
        if (request.ParentCommentId.HasValue)
        {
            var parentComment = await _context.Set<Comment>()
                .FirstOrDefaultAsync(c => c.Id == request.ParentCommentId.Value && c.ItemId == request.ItemId, cancellationToken);

            if (parentComment == null)
            {
                throw new KeyNotFoundException("Parent comment not found or does not belong to this item.");
            }
        }

        // Create comment
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            ItemId = request.ItemId,
            UserId = request.UserId,
            Body = request.Body,
            ParentCommentId = request.ParentCommentId,
            IsApproved = false // Requires moderation
        };

        _context.Set<Comment>().Add(comment);
        await _context.SaveChangesAsync(cancellationToken);

        var user = await _context.Set<Domain.Users.User>()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        return new CommentDto
        {
            Id = comment.Id,
            ItemId = comment.ItemId,
            UserId = comment.UserId,
            UserFullName = user?.FullName ?? string.Empty,
            Body = comment.Body,
            ParentCommentId = comment.ParentCommentId,
            IsApproved = comment.IsApproved,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }
}

