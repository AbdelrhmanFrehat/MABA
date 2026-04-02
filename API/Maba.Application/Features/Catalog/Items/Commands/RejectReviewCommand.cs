using MediatR;
using Maba.Application.Features.Catalog.Items.DTOs;

namespace Maba.Application.Features.Catalog.Items.Commands;

public class RejectReviewCommand : IRequest<ReviewDto>
{
    public Guid ReviewId { get; set; }
    public string? Reason { get; set; }
}
