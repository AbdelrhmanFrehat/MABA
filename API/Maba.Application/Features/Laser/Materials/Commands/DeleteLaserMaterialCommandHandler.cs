using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Laser;

namespace Maba.Application.Features.Laser.Materials.Commands;

public class DeleteLaserMaterialCommandHandler : IRequestHandler<DeleteLaserMaterialCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteLaserMaterialCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteLaserMaterialCommand request, CancellationToken cancellationToken)
    {
        var material = await _context.Set<LaserMaterial>()
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (material == null) return false;

        _context.Set<LaserMaterial>().Remove(material);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
