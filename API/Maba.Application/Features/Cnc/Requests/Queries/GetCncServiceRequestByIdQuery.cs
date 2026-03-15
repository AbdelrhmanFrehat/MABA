using MediatR;
using Maba.Application.Features.Cnc.DTOs;

namespace Maba.Application.Features.Cnc.Requests.Queries;

public class GetCncServiceRequestByIdQuery : IRequest<CncServiceRequestDto?>
{
    public Guid Id { get; set; }
}
