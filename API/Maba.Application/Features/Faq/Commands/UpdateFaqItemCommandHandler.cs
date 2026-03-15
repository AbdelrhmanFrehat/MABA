using MediatR;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Faq;
using Microsoft.EntityFrameworkCore;

namespace Maba.Application.Features.Faq.Commands;

public class UpdateFaqItemCommandHandler : IRequestHandler<UpdateFaqItemCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateFaqItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateFaqItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.Set<FaqItem>().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (item == null) return false;

        item.Category = request.Category;
        item.QuestionEn = request.QuestionEn;
        item.QuestionAr = request.QuestionAr;
        item.AnswerEn = request.AnswerEn;
        item.AnswerAr = request.AnswerAr;
        item.IsActive = request.IsActive;
        item.IsFeatured = request.IsFeatured;
        item.SortOrder = request.SortOrder;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
