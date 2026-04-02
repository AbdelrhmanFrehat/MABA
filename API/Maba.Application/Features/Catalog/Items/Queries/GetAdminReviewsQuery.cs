using MediatR;
using Maba.Application.Features.Catalog.Items.DTOs;

namespace Maba.Application.Features.Catalog.Items.Queries;

public class GetAdminReviewsQuery : IRequest<AdminReviewListResponse>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Status { get; set; }
    public int? Rating { get; set; }
}
