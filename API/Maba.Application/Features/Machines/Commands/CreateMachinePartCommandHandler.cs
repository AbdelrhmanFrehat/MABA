using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Machines.Commands;
using Maba.Application.Features.Machines.DTOs;
using Maba.Domain.Machines;

namespace Maba.Application.Features.Machines.Handlers;

public class CreateMachinePartCommandHandler : IRequestHandler<CreateMachinePartCommand, MachinePartDto>
{
    private readonly IApplicationDbContext _context;

    public CreateMachinePartCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MachinePartDto> Handle(CreateMachinePartCommand request, CancellationToken cancellationToken)
    {
        var machineExists = await _context.Set<Machine>()
            .AnyAsync(m => m.Id == request.MachineId, cancellationToken);

        if (!machineExists)
        {
            throw new KeyNotFoundException("Machine not found");
        }

        var part = new MachinePart
        {
            Id = Guid.NewGuid(),
            MachineId = request.MachineId,
            PartNameEn = request.PartNameEn,
            PartNameAr = request.PartNameAr,
            PartCode = request.PartCode
        };

        _context.Set<MachinePart>().Add(part);
        await _context.SaveChangesAsync(cancellationToken);

        return new MachinePartDto
        {
            Id = part.Id,
            MachineId = part.MachineId,
            PartNameEn = part.PartNameEn,
            PartNameAr = part.PartNameAr,
            PartCode = part.PartCode,
            CreatedAt = part.CreatedAt,
            UpdatedAt = part.UpdatedAt
        };
    }
}

