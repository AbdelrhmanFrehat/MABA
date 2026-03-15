using MediatR;
using Maba.Application.Features.Laser.DTOs;

namespace Maba.Application.Features.Laser.Materials.Queries;

public class GetLaserMaterialByIdQuery : IRequest<LaserMaterialDto?>
{
    public Guid Id { get; set; }
}
