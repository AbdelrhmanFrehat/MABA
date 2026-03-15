using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Machines.Commands;
using Maba.Domain.Machines;

namespace Maba.Application.Features.Machines.Handlers;

public class DeleteMachineCommandHandler : IRequestHandler<DeleteMachineCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteMachineCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteMachineCommand request, CancellationToken cancellationToken)
    {
        var machine = await _context.Set<Machine>()
            .Include(m => m.Parts)
            .Include(m => m.ItemMachineLinks)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (machine == null)
        {
            throw new KeyNotFoundException("Machine not found");
        }

        // Remove parts
        foreach (var part in machine.Parts.ToList())
        {
            _context.Set<MachinePart>().Remove(part);
        }

        // Remove item machine links
        foreach (var link in machine.ItemMachineLinks.ToList())
        {
            _context.Set<ItemMachineLink>().Remove(link);
        }

        _context.Set<Machine>().Remove(machine);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

