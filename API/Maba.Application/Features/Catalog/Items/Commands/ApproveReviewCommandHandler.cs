using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Items.Commands;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Items.Handlers;

public class ApproveReviewCommandHandler : IRequestHandler<ApproveReviewCommand, ReviewDto>
{
    private readonly IApplicationDbContext _context;

    public ApproveReviewCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReviewDto> Handle(ApproveReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _context.Set<Review>()
            .Include(r => r.Item)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review == null)
        {
            throw new KeyNotFoundException("Review not found.");
        }

        var wasApproved = review.IsApproved;
        review.IsApproved = true;
        review.UpdatedAt = DateTime.UtcNow;

        // If this is a new approval, recalculate average rating
        if (!wasApproved)
        {
            var approvedReviews = await _context.Set<Review>()
                .Where(r => r.ItemId == review.ItemId && r.IsApproved)
                .ToListAsync(cancellationToken);

            var item = review.Item;
            item.AverageRating = approvedReviews.Any() 
                ? (decimal)approvedReviews.Average(r => r.Rating) 
                : 0;
            item.ReviewsCount = approvedReviews.Count;
            item.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ReviewDto
        {
            Id = review.Id,
            ItemId = review.ItemId,
            UserId = review.UserId,
            UserFullName = review.User.FullName,
            Rating = review.Rating,
            Title = review.Title,
            Body = review.Body,
            IsApproved = review.IsApproved,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }
}

