using MediatR;
using Maba.Application.Features.Cnc.DTOs;

namespace Maba.Application.Features.Cnc.Materials.Queries;

public class GetAllCncMaterialsQuery : IRequest<List<CncMaterialDto>>
{
    public bool? ActiveOnly { get; set; } = null;
    public string? Type { get; set; } = null;
    public bool ExcludeMetal { get; set; } = false;
    public bool? IsPcbOnly { get; set; } = null;
}
