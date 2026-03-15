using MediatR;

namespace Maba.Application.Features.Printing.Materials.Commands;

public class DeleteMaterialCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
