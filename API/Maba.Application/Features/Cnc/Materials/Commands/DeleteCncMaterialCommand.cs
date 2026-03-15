using MediatR;

namespace Maba.Application.Features.Cnc.Materials.Commands;

public class DeleteCncMaterialCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
