using MediatR;
using Maba.Application.Features.ControlCenter.Instances.DTOs;

namespace Maba.Application.Features.ControlCenter.Instances.Queries;

public class GetInstanceByIdQuery : IRequest<ControlCenterInstanceDto?>
{
    public Guid Id { get; set; }
    public Guid OrgId { get; set; }
}

