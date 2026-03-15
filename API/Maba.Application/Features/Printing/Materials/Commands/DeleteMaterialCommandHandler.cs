using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.Materials.Commands;

public class DeleteMaterialCommandHandler : IRequestHandler<DeleteMaterialCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteMaterialCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteMaterialCommand request, CancellationToken cancellationToken)
    {
        var material = await _context.Set<Material>()
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (material == null)
        {
            return false;
        }

        _context.Set<Material>().Remove(material);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
