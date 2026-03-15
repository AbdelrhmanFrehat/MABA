using MediatR;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Faq;
using Microsoft.EntityFrameworkCore;

namespace Maba.Application.Features.Faq.Commands;

public class DeleteFaqItemCommandHandler : IRequestHandler<DeleteFaqItemCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteFaqItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteFaqItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.Set<FaqItem>().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (item == null) return false;

        _context.Set<FaqItem>().Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
