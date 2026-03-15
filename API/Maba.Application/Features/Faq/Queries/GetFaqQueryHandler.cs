using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Faq.DTOs;
using Maba.Domain.Faq;

namespace Maba.Application.Features.Faq.Queries;

public class GetFaqQueryHandler : IRequestHandler<GetFaqQuery, List<FaqItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFaqQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<FaqItemDto>> Handle(GetFaqQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<FaqItem>()
            .Where(x => x.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.ToLower();
            query = query.Where(x =>
                x.QuestionEn.ToLower().Contains(term) ||
                (x.QuestionAr != null && x.QuestionAr.ToLower().Contains(term)) ||
                x.AnswerEn.ToLower().Contains(term) ||
                (x.AnswerAr != null && x.AnswerAr.ToLower().Contains(term)));
        }

        if (request.Category.HasValue)
        {
            query = query.Where(x => x.Category == request.Category.Value);
        }

        if (request.IsFeaturedOnly == true)
        {
            query = query.Where(x => x.IsFeatured);
        }

        var items = await query
            .OrderByDescending(x => x.IsFeatured)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.QuestionEn)
            .ToListAsync(cancellationToken);

        return items.Select(Map).ToList();
    }

    private static FaqItemDto Map(FaqItem x) => new()
    {
        Id = x.Id,
        Category = x.Category,
        QuestionEn = x.QuestionEn,
        QuestionAr = x.QuestionAr,
        AnswerEn = x.AnswerEn,
        AnswerAr = x.AnswerAr,
        IsActive = x.IsActive,
        IsFeatured = x.IsFeatured,
        SortOrder = x.SortOrder,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt
    };
}
