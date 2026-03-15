using MediatR;

namespace Maba.Application.Features.Catalog.Items.Commands;

public class DeleteReviewCommand : IRequest<Unit>
{
    public Guid ReviewId { get; set; }
}

