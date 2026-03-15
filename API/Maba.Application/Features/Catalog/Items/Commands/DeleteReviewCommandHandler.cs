using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Items.Commands;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Items.Handlers;

public class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteReviewCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _context.Set<Review>()
            .Include(r => r.Item)
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review == null)
        {
            throw new KeyNotFoundException("Review not found.");
        }

        var itemId = review.ItemId;
        var wasApproved = review.IsApproved;

        _context.Set<Review>().Remove(review);

        // Recalculate average rating if review was approved
        if (wasApproved)
        {
            var approvedReviews = await _context.Set<Review>()
                .Where(r => r.ItemId == itemId && r.IsApproved)
                .ToListAsync(cancellationToken);

            var item = review.Item;
            item.AverageRating = approvedReviews.Any()
                ? (decimal)approvedReviews.Average(r => r.Rating)
                : 0;
            item.ReviewsCount = approvedReviews.Count;
            item.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

