using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Machines.Commands;
using Maba.Application.Features.Machines.DTOs;
using Maba.Domain.Machines;

namespace Maba.Application.Features.Machines.Handlers;

public class UpdateMachinePartCommandHandler : IRequestHandler<UpdateMachinePartCommand, MachinePartDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateMachinePartCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MachinePartDto> Handle(UpdateMachinePartCommand request, CancellationToken cancellationToken)
    {
        var part = await _context.Set<MachinePart>()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Machine part {request.Id} not found.");

        part.MachineId = request.MachineId;
        part.PartNameEn = request.PartNameEn;
        part.PartNameAr = request.PartNameAr;
        part.PartCode = request.PartCode;
        part.Price = request.Price;
        part.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new MachinePartDto
        {
            Id = part.Id,
            MachineId = part.MachineId,
            PartNameEn = part.PartNameEn,
            PartNameAr = part.PartNameAr,
            PartCode = part.PartCode,
            Price = part.Price,
            InventoryId = part.InventoryId,
            ImageId = part.ImageId,
            CreatedAt = part.CreatedAt,
            UpdatedAt = part.UpdatedAt
        };
    }
}
