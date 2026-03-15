using MediatR;
using Maba.Application.Features.Catalog.Items.DTOs;

namespace Maba.Application.Features.Catalog.Items.Queries;

public class GetItemReviewsQuery : IRequest<List<ReviewDto>>
{
    public Guid ItemId { get; set; }
    public bool? IsApproved { get; set; } // null = all, true = approved only, false = pending only
}

