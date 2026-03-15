using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Machines.Commands;
using Maba.Application.Features.Machines.DTOs;
using Maba.Domain.Machines;

namespace Maba.Application.Features.Machines.Handlers;

public class CreateMachineCommandHandler : IRequestHandler<CreateMachineCommand, MachineDto>
{
    private readonly IApplicationDbContext _context;

    public CreateMachineCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MachineDto> Handle(CreateMachineCommand request, CancellationToken cancellationToken)
    {
        var machine = new Machine
        {
            Id = Guid.NewGuid(),
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            Manufacturer = request.Manufacturer,
            Model = request.Model,
            YearFrom = request.YearFrom,
            YearTo = request.YearTo
        };

        _context.Set<Machine>().Add(machine);
        await _context.SaveChangesAsync(cancellationToken);

        return new MachineDto
        {
            Id = machine.Id,
            NameEn = machine.NameEn,
            NameAr = machine.NameAr,
            Manufacturer = machine.Manufacturer,
            Model = machine.Model,
            YearFrom = machine.YearFrom,
            YearTo = machine.YearTo,
            CreatedAt = machine.CreatedAt,
            UpdatedAt = machine.UpdatedAt
        };
    }
}

