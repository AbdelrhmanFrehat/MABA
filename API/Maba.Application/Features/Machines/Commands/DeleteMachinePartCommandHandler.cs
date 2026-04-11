using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Machines.Commands;
using Maba.Domain.Machines;

namespace Maba.Application.Features.Machines.Handlers;

public class DeleteMachinePartCommandHandler : IRequestHandler<DeleteMachinePartCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteMachinePartCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteMachinePartCommand request, CancellationToken cancellationToken)
    {
        var part = await _context.Set<MachinePart>()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Machine part {request.Id} not found.");

        _context.Set<MachinePart>().Remove(part);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
