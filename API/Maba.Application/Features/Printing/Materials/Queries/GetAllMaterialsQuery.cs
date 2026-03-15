using MediatR;
using Maba.Application.Features.Printing.DTOs;

namespace Maba.Application.Features.Printing.Materials.Queries;

public class GetAllMaterialsQuery : IRequest<List<MaterialDto>>
{
}

