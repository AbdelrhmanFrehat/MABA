using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Cnc;

namespace Maba.Application.Features.Cnc.Materials.Commands;

public class DeleteCncMaterialCommandHandler : IRequestHandler<DeleteCncMaterialCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteCncMaterialCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteCncMaterialCommand request, CancellationToken cancellationToken)
    {
        var material = await _context.Set<CncMaterial>()
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (material == null)
        {
            return false;
        }

        _context.Set<CncMaterial>().Remove(material);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
