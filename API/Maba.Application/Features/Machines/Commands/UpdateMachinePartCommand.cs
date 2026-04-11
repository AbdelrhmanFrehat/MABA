using MediatR;
using Maba.Application.Features.Machines.DTOs;

namespace Maba.Application.Features.Machines.Commands;

public class UpdateMachinePartCommand : IRequest<MachinePartDto>
{
    public Guid Id { get; set; }
    public Guid MachineId { get; set; }
    public string PartNameEn { get; set; } = string.Empty;
    public string PartNameAr { get; set; } = string.Empty;
    public string? PartCode { get; set; }
    public decimal? Price { get; set; }
}
