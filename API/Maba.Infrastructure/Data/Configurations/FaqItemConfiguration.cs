using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Faq;

namespace Maba.Infrastructure.Data.Configurations;

public class FaqItemConfiguration : IEntityTypeConfiguration<FaqItem>
{
    public void Configure(EntityTypeBuilder<FaqItem> builder)
    {
        builder.ToTable("FaqItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuestionEn).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.QuestionAr).HasMaxLength(1000);
        builder.Property(x => x.AnswerEn).IsRequired();
        builder.Property(x => x.AnswerAr);
        builder.Property(x => x.Category).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.IsFeatured).HasDefaultValue(false);
        builder.Property(x => x.SortOrder).HasDefaultValue(0);
    }
}
