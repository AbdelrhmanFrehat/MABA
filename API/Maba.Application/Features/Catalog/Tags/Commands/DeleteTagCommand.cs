using MediatR;

namespace Maba.Application.Features.Catalog.Tags.Commands;

public class DeleteTagCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}

