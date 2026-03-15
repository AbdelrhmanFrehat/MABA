using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cms.Pages.Commands;
using Maba.Domain.Cms;

namespace Maba.Application.Features.Cms.Pages.Handlers;

public class DeletePageCommandHandler : IRequestHandler<DeletePageCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeletePageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeletePageCommand request, CancellationToken cancellationToken)
    {
        var page = await _context.Set<Page>()
            .Include(p => p.DraftSections)
            .Include(p => p.PublishedSections)
            .Include(p => p.Versions)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (page == null)
        {
            throw new KeyNotFoundException("Page not found.");
        }

        // Prevent deletion of home page
        if (page.IsHome)
        {
            throw new InvalidOperationException("Cannot delete the home page.");
        }

        // Delete related entities (cascade)
        _context.Set<PageSectionDraft>().RemoveRange(page.DraftSections);
        _context.Set<PageSectionPublished>().RemoveRange(page.PublishedSections);
        _context.Set<PageVersion>().RemoveRange(page.Versions);

        _context.Set<Page>().Remove(page);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

