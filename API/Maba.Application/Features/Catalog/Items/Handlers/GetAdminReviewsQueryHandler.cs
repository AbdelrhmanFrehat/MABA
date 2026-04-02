using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Application.Features.Catalog.Items.Queries;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Items.Handlers;

public class GetAdminReviewsQueryHandler : IRequestHandler<GetAdminReviewsQuery, AdminReviewListResponse>
{
    private readonly IApplicationDbContext _context;

    public GetAdminReviewsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminReviewListResponse> Handle(GetAdminReviewsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Review>()
            .AsNoTracking()
            .Include(r => r.Item)
            .Include(r => r.User)
            .AsQueryable();

        if (request.Rating.HasValue)
        {
            query = query.Where(r => r.Rating == request.Rating.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var s = request.Status.Trim();
            query = s switch
            {
                "Approved" => query.Where(r => r.IsApproved),
                "Rejected" => query.Where(r => r.IsRejected),
                "Pending" => query.Where(r => !r.IsApproved && !r.IsRejected),
                _ => query
            };
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var averageRating = totalCount > 0
            ? (decimal)await query.AverageAsync(r => (double)r.Rating, cancellationToken)
            : 0m;

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var pageEntities = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = pageEntities.Select(r => new AdminReviewListItemDto
        {
            Id = r.Id,
            ItemId = r.ItemId,
            ItemNameEn = r.Item.NameEn,
            ItemNameAr = r.Item.NameAr,
            UserId = r.UserId,
            UserName = r.User.FullName,
            Rating = r.Rating,
            Title = r.Title,
            Comment = r.Body,
            Status = r.IsApproved ? "Approved" : r.IsRejected ? "Rejected" : "Pending",
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            IsVerifiedPurchase = false,
            HelpfulCount = 0,
            Replies = new List<object>()
        }).ToList();

        return new AdminReviewListResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            AverageRating = averageRating
        };
    }
}
