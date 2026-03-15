using MediatR;
using Maba.Application.Features.Laser.DTOs;

namespace Maba.Application.Features.Laser.Requests.Queries;

public class GetLaserServiceRequestByIdQuery : IRequest<LaserServiceRequestDto?>
{
    public Guid Id { get; set; }
}
