using MediatR;
using Maba.Application.Features.Printing.DTOs;

namespace Maba.Application.Features.Printing.Materials.Queries;

public class GetMaterialByIdQuery : IRequest<MaterialDto?>
{
    public Guid Id { get; set; }
}
