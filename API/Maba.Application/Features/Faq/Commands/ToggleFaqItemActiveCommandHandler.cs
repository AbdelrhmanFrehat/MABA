using MediatR;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Faq;
using Microsoft.EntityFrameworkCore;

namespace Maba.Application.Features.Faq.Commands;

public class ToggleFaqItemActiveCommandHandler : IRequestHandler<ToggleFaqItemActiveCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ToggleFaqItemActiveCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ToggleFaqItemActiveCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.Set<FaqItem>().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (item == null) return false;

        item.IsActive = !item.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
