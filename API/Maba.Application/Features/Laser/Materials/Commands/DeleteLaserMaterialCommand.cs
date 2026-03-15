using MediatR;

namespace Maba.Application.Features.Laser.Materials.Commands;

public class DeleteLaserMaterialCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
