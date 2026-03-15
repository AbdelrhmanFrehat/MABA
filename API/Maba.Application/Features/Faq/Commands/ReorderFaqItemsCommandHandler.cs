using MediatR;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Faq;
using Microsoft.EntityFrameworkCore;

namespace Maba.Application.Features.Faq.Commands;

public class ReorderFaqItemsCommandHandler : IRequestHandler<ReorderFaqItemsCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ReorderFaqItemsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ReorderFaqItemsCommand request, CancellationToken cancellationToken)
    {
        var ids = request.Items.Select(x => x.Id).ToList();
        var items = await _context.Set<FaqItem>()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);

        foreach (var orderItem in request.Items)
        {
            var item = items.FirstOrDefault(x => x.Id == orderItem.Id);
            if (item != null)
                item.SortOrder = orderItem.SortOrder;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
