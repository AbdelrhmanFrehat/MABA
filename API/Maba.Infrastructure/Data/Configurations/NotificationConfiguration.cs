using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Common;

namespace Maba.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(n => n.TitleEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.TitleAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.MessageEn)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(n => n.MessageAr)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(n => n.LinkUrl)
            .HasMaxLength(500);

        builder.Property(n => n.Icon)
            .HasMaxLength(100);

        builder.Property(n => n.RelatedEntityType)
            .HasMaxLength(100);

        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.IsRead);
        builder.HasIndex(n => n.CreatedAt);
        builder.HasIndex(n => new { n.UserId, n.IsRead });
        builder.HasIndex(n => new { n.RelatedEntityType, n.RelatedEntityId });

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

