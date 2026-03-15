using MediatR;
using Maba.Application.Features.Laser.DTOs;
using Maba.Domain.Laser;

namespace Maba.Application.Features.Laser.Requests.Queries;

public class GetAllLaserServiceRequestsQuery : IRequest<List<LaserServiceRequestDto>>
{
    public LaserServiceRequestStatus? Status { get; set; }
    public int? Limit { get; set; }
    public int? Offset { get; set; }
}
