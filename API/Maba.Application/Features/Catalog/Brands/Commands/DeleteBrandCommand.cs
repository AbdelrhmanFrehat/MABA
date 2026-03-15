using MediatR;

namespace Maba.Application.Features.Catalog.Brands.Commands;

public class DeleteBrandCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}

