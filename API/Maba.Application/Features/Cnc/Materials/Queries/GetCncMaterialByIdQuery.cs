using MediatR;
using Maba.Application.Features.Cnc.DTOs;

namespace Maba.Application.Features.Cnc.Materials.Queries;

public class GetCncMaterialByIdQuery : IRequest<CncMaterialDto?>
{
    public Guid Id { get; set; }
}
