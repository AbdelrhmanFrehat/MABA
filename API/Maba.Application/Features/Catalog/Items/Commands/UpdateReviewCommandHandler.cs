using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Items.Commands;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Items.Handlers;

public class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand, ReviewDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateReviewCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReviewDto> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _context.Set<Review>()
            .Include(r => r.Item)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review == null)
        {
            throw new KeyNotFoundException("Review not found.");
        }

        if (request.Rating.HasValue)
        {
            if (request.Rating.Value < 1 || request.Rating.Value > 5)
            {
                throw new ArgumentException("Rating must be between 1 and 5.");
            }
            review.Rating = request.Rating.Value;
        }

        if (request.Title != null)
        {
            review.Title = request.Title;
        }

        if (request.Body != null)
        {
            review.Body = request.Body;
        }

        review.UpdatedAt = DateTime.UtcNow;

        // Recalculate average rating if review is approved
        if (review.IsApproved)
        {
            var approvedReviews = await _context.Set<Review>()
                .Where(r => r.ItemId == review.ItemId && r.IsApproved)
                .ToListAsync(cancellationToken);

            review.Item.AverageRating = approvedReviews.Any()
                ? (decimal)approvedReviews.Average(r => r.Rating)
                : 0;
            review.Item.ReviewsCount = approvedReviews.Count;
            review.Item.UpdatedAt = DateTime.UtcNow;
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

