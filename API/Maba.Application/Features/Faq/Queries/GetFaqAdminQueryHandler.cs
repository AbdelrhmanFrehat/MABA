using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Faq.DTOs;
using Maba.Domain.Faq;

namespace Maba.Application.Features.Faq.Queries;

public class GetFaqAdminQueryHandler : IRequestHandler<GetFaqAdminQuery, List<FaqItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFaqAdminQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<FaqItemDto>> Handle(GetFaqAdminQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<FaqItem>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.ToLower();
            query = query.Where(x =>
                x.QuestionEn.ToLower().Contains(term) ||
                (x.QuestionAr != null && x.QuestionAr.ToLower().Contains(term)));
        }

        if (request.Category.HasValue)
        {
            query = query.Where(x => x.Category == request.Category.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.QuestionEn)
            .ToListAsync(cancellationToken);

        return items.Select(x => new FaqItemDto
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
        }).ToList();
    }
}
