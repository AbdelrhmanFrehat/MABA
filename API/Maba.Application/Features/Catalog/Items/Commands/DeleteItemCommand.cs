using MediatR;

namespace Maba.Application.Features.Catalog.Items.Commands;

public class DeleteItemCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}

