using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Machines.Commands;
using Maba.Application.Features.Machines.DTOs;
using Maba.Domain.Machines;

namespace Maba.Application.Features.Machines.Handlers;

public class CreateItemMachineLinkCommandHandler : IRequestHandler<CreateItemMachineLinkCommand, ItemMachineLinkDto>
{
    private readonly IApplicationDbContext _context;

    public CreateItemMachineLinkCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ItemMachineLinkDto> Handle(CreateItemMachineLinkCommand request, CancellationToken cancellationToken)
    {
        // Validate item exists
        var itemExists = await _context.Set<Domain.Catalog.Item>()
            .AnyAsync(i => i.Id == request.ItemId, cancellationToken);

        if (!itemExists)
        {
            throw new KeyNotFoundException("Item not found");
        }

        // Validate machine exists
        var machine = await _context.Set<Machine>()
            .FirstOrDefaultAsync(m => m.Id == request.MachineId, cancellationToken);

        if (machine == null)
        {
            throw new KeyNotFoundException("Machine not found");
        }

        // Validate part if provided
        if (request.MachinePartId.HasValue)
        {
            var partExists = await _context.Set<MachinePart>()
                .AnyAsync(p => p.Id == request.MachinePartId.Value && p.MachineId == request.MachineId, cancellationToken);

            if (!partExists)
            {
                throw new KeyNotFoundException("Machine part not found or does not belong to the specified machine");
            }
        }

        var link = new ItemMachineLink
        {
            Id = Guid.NewGuid(),
            ItemId = request.ItemId,
            MachineId = request.MachineId,
            MachinePartId = request.MachinePartId
        };

        _context.Set<ItemMachineLink>().Add(link);
        await _context.SaveChangesAsync(cancellationToken);

        var part = request.MachinePartId.HasValue
            ? await _context.Set<MachinePart>().FirstOrDefaultAsync(p => p.Id == request.MachinePartId.Value, cancellationToken)
            : null;

        return new ItemMachineLinkDto
        {
            Id = link.Id,
            ItemId = link.ItemId,
            MachineId = link.MachineId,
            MachinePartId = link.MachinePartId,
            MachineNameEn = machine.NameEn,
            PartNameEn = part?.PartNameEn,
            CreatedAt = link.CreatedAt,
            UpdatedAt = link.UpdatedAt
        };
    }
}

