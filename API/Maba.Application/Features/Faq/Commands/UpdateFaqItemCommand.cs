using MediatR;
using Maba.Application.Features.Faq.DTOs;
using Maba.Domain.Faq;

namespace Maba.Application.Features.Faq.Commands;

public class UpdateFaqItemCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public FaqCategory Category { get; set; }
    public string QuestionEn { get; set; } = string.Empty;
    public string? QuestionAr { get; set; }
    public string AnswerEn { get; set; } = string.Empty;
    public string? AnswerAr { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; }
}
