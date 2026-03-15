using MediatR;
using Maba.Application.Features.Laser.DTOs;

namespace Maba.Application.Features.Laser.Materials.Queries;

public class GetAllLaserMaterialsQuery : IRequest<List<LaserMaterialDto>>
{
    public bool? ActiveOnly { get; set; } = null;
    public string? Type { get; set; } = null;
    public bool ExcludeMetal { get; set; } = false;
}
