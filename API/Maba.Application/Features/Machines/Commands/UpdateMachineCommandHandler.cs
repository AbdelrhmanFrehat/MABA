using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Machines.Commands;
using Maba.Application.Features.Machines.DTOs;
using Maba.Domain.Machines;

namespace Maba.Application.Features.Machines.Handlers;

public class UpdateMachineCommandHandler : IRequestHandler<UpdateMachineCommand, MachineDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateMachineCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MachineDto> Handle(UpdateMachineCommand request, CancellationToken cancellationToken)
    {
        var machine = await _context.Set<Machine>()
            .Include(m => m.Parts)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (machine == null)
        {
            throw new KeyNotFoundException("Machine not found");
        }

        machine.NameEn = request.NameEn;
        machine.NameAr = request.NameAr;
        machine.Manufacturer = request.Manufacturer;
        machine.Model = request.Model;
        machine.YearFrom = request.YearFrom;
        machine.YearTo = request.YearTo;
        machine.UpdatedAt = DateTime.UtcNow;

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
            Parts = machine.Parts.Select(p => new MachinePartDto
            {
                Id = p.Id,
                MachineId = p.MachineId,
                PartNameEn = p.PartNameEn,
                PartNameAr = p.PartNameAr,
                PartCode = p.PartCode,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList(),
            CreatedAt = machine.CreatedAt,
            UpdatedAt = machine.UpdatedAt
        };
    }
}

