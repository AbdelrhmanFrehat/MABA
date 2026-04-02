using Maba.Domain.Faq;

namespace Maba.Infrastructure.Data.Seeding;

/// <summary>Large FAQ seed dataset (EN + AR). Merged into DB by question text to avoid duplicates on re-run.</summary>
public static partial class FaqContent
{
    public static List<FaqItem> All()
    {
        var list = new List<FaqItem>();
        list.AddRange(Print3d());
        list.AddRange(Laser());
        list.AddRange(Cnc());
        list.AddRange(Software());
        list.AddRange(OrdersShipping());
        list.AddRange(Payments());
        list.AddRange(Support());
        list.AddRange(General());
        return list;
    }

    private static FaqItem F(
        FaqCategory category,
        int sortOrder,
        string questionEn,
        string questionAr,
        string answerEn,
        string answerAr,
        bool featured = false)
    {
        return new FaqItem
        {
            Id = Guid.NewGuid(),
            Category = category,
            QuestionEn = questionEn,
            QuestionAr = questionAr,
            AnswerEn = answerEn,
            AnswerAr = answerAr,
            IsActive = true,
            IsFeatured = featured,
            SortOrder = sortOrder,
            CreatedAt = DateTime.UtcNow
        };
    }
}
