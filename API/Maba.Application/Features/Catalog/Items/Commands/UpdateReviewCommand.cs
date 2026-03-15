using MediatR;
using Maba.Application.Features.Catalog.Items.DTOs;

namespace Maba.Application.Features.Catalog.Items.Commands;

public class UpdateReviewCommand : IRequest<ReviewDto>
{
    public Guid ReviewId { get; set; }
    public int? Rating { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
}

