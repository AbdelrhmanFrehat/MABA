using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.PrintQualityProfiles.Commands;

public class DeletePrintQualityProfileCommandHandler : IRequestHandler<DeletePrintQualityProfileCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeletePrintQualityProfileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeletePrintQualityProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _context.Set<PrintQualityProfile>()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (profile == null)
        {
            return false;
        }

        _context.Set<PrintQualityProfile>().Remove(profile);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
