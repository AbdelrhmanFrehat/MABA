using MediatR;

namespace Maba.Application.Features.Catalog.Categories.Commands;

public class DeleteCategoryCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}

