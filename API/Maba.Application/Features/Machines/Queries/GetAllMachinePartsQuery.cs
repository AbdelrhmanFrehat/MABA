using MediatR;
using Maba.Application.Features.Machines.DTOs;

namespace Maba.Application.Features.Machines.Queries;

public class GetAllMachinePartsQuery : IRequest<GetAllMachinePartsResult>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public Guid? MachineId { get; set; }
    public string? Search { get; set; }
}

public class GetAllMachinePartsResult
{
    public List<MachinePartDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
