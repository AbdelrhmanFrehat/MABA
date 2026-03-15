using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Faq.DTOs;
using Maba.Domain.Faq;

namespace Maba.Application.Features.Faq.Queries;

public class GetFaqItemByIdQueryHandler : IRequestHandler<GetFaqItemByIdQuery, FaqItemDto?>
{
    private readonly IApplicationDbContext _context;

    public GetFaqItemByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FaqItemDto?> Handle(GetFaqItemByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _context.Set<FaqItem>()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (item == null) return null;

        return new FaqItemDto
        {
            Id = item.Id,
            Category = item.Category,
            QuestionEn = item.QuestionEn,
            QuestionAr = item.QuestionAr,
            AnswerEn = item.AnswerEn,
            AnswerAr = item.AnswerAr,
            IsActive = item.IsActive,
            IsFeatured = item.IsFeatured,
            SortOrder = item.SortOrder,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }
}
