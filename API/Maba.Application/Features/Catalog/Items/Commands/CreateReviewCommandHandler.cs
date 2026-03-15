using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Items.Commands;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Items.Handlers;

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, ReviewDto>
{
    private readonly IApplicationDbContext _context;

    public CreateReviewCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReviewDto> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        // Check if item exists
        var item = await _context.Set<Item>()
            .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);

        if (item == null)
        {
            throw new KeyNotFoundException("Item not found.");
        }

        // Check if user already reviewed this item
        var existingReview = await _context.Set<Review>()
            .FirstOrDefaultAsync(r => r.ItemId == request.ItemId && r.UserId == request.UserId, cancellationToken);

        if (existingReview != null)
        {
            throw new InvalidOperationException("User has already reviewed this item.");
        }

        // Validate rating
        if (request.Rating < 1 || request.Rating > 5)
        {
            throw new ArgumentException("Rating must be between 1 and 5.");
        }

        // Create review
        var review = new Review
        {
            Id = Guid.NewGuid(),
            ItemId = request.ItemId,
            UserId = request.UserId,
            Rating = request.Rating,
            Title = request.Title,
            Body = request.Body,
            IsApproved = false // Requires moderation
        };

        _context.Set<Review>().Add(review);
        await _context.SaveChangesAsync(cancellationToken);

        // Recalculate average rating (will be done when review is approved)
        // For now, just return the review

        var user = await _context.Set<Domain.Users.User>()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        return new ReviewDto
        {
            Id = review.Id,
            ItemId = review.ItemId,
            UserId = review.UserId,
            UserFullName = user?.FullName ?? string.Empty,
            Rating = review.Rating,
            Title = review.Title,
            Body = review.Body,
            IsApproved = review.IsApproved,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }
}

