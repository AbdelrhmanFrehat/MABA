using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Items.Queries;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Items.Handlers;

public class GetItemCommentsQueryHandler : IRequestHandler<GetItemCommentsQuery, List<CommentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetItemCommentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CommentDto>> Handle(GetItemCommentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Comment>()
            .Include(c => c.User)
            .Include(c => c.Replies)
            .ThenInclude(r => r.User)
            .Where(c => c.ItemId == request.ItemId && c.ParentCommentId == null); // Only top-level comments

        if (request.IsApproved.HasValue)
        {
            query = query.Where(c => c.IsApproved == request.IsApproved.Value);
        }

        var comments = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return comments.Select(c => new CommentDto
        {
            Id = c.Id,
            ItemId = c.ItemId,
            UserId = c.UserId,
            UserFullName = c.User.FullName,
            Body = c.Body,
            ParentCommentId = c.ParentCommentId,
            IsApproved = c.IsApproved,
            Replies = c.Replies
                .Where(r => !request.IsApproved.HasValue || r.IsApproved == request.IsApproved.Value)
                .Select(r => new CommentDto
                {
                    Id = r.Id,
                    ItemId = r.ItemId,
                    UserId = r.UserId,
                    UserFullName = r.User.FullName,
                    Body = r.Body,
                    ParentCommentId = r.ParentCommentId,
                    IsApproved = r.IsApproved,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .OrderBy(r => r.CreatedAt)
                .ToList(),
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList();
    }
}

