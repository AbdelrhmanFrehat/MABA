using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Items.Queries;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Items.Handlers;

public class GetItemReviewsQueryHandler : IRequestHandler<GetItemReviewsQuery, List<ReviewDto>>
{
    private readonly IApplicationDbContext _context;

    public GetItemReviewsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReviewDto>> Handle(GetItemReviewsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Review>()
            .Include(r => r.User)
            .Where(r => r.ItemId == request.ItemId);

        if (request.IsApproved.HasValue)
        {
            query = query.Where(r => r.IsApproved == request.IsApproved.Value);
        }

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return reviews.Select(r => new ReviewDto
        {
            Id = r.Id,
            ItemId = r.ItemId,
            UserId = r.UserId,
            UserFullName = r.User.FullName,
            Rating = r.Rating,
            Title = r.Title,
            Body = r.Body,
            IsApproved = r.IsApproved,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList();
    }
}

