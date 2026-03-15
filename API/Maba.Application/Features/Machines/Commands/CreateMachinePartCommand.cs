using MediatR;
using Maba.Application.Features.Machines.DTOs;

namespace Maba.Application.Features.Machines.Commands;

public class CreateMachinePartCommand : IRequest<MachinePartDto>
{
    public Guid MachineId { get; set; }
    public string PartNameEn { get; set; } = string.Empty;
    public string PartNameAr { get; set; } = string.Empty;
    public string? PartCode { get; set; }
}

