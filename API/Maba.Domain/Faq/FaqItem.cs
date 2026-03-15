using Maba.Domain.Common;

namespace Maba.Domain.Faq;

public class FaqItem : BaseEntity
{
    public FaqCategory Category { get; set; }
    public string QuestionEn { get; set; } = string.Empty;
    public string? QuestionAr { get; set; }
    public string AnswerEn { get; set; } = string.Empty;
    public string? AnswerAr { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; }
}
