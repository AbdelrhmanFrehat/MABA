using MediatR;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Faq.DTOs;
using Maba.Domain.Faq;

namespace Maba.Application.Features.Faq.Commands;

public class CreateFaqItemCommandHandler : IRequestHandler<CreateFaqItemCommand, FaqItemDto>
{
    private readonly IApplicationDbContext _context;

    public CreateFaqItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FaqItemDto> Handle(CreateFaqItemCommand request, CancellationToken cancellationToken)
    {
        var item = new FaqItem
        {
            Id = Guid.NewGuid(),
            Category = request.Category,
            QuestionEn = request.QuestionEn,
            QuestionAr = request.QuestionAr,
            AnswerEn = request.AnswerEn,
            AnswerAr = request.AnswerAr,
            IsActive = request.IsActive,
            IsFeatured = request.IsFeatured,
            SortOrder = request.SortOrder
        };

        _context.Set<FaqItem>().Add(item);
        await _context.SaveChangesAsync(cancellationToken);

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
