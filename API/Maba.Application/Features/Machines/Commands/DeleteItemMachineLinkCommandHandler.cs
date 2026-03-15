using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Machines.Commands;
using Maba.Domain.Machines;

namespace Maba.Application.Features.Machines.Handlers;

public class DeleteItemMachineLinkCommandHandler : IRequestHandler<DeleteItemMachineLinkCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteItemMachineLinkCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteItemMachineLinkCommand request, CancellationToken cancellationToken)
    {
        var link = await _context.Set<ItemMachineLink>()
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (link == null)
        {
            throw new KeyNotFoundException("Item-machine link not found");
        }

        _context.Set<ItemMachineLink>().Remove(link);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
