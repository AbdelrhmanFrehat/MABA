using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Announcements;

namespace Maba.Infrastructure.Data.Configurations;

public class AppAnnouncementItemConfiguration : IEntityTypeConfiguration<AppAnnouncementItem>
{
    public void Configure(EntityTypeBuilder<AppAnnouncementItem> builder)
    {
        builder.ToTable("AppAnnouncementItems");
        builder.ConfigureBaseEntity<AppAnnouncementItem>();

        builder.Property(x => x.Message).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.Type).HasMaxLength(50);
        builder.Property(x => x.TargetPlatform).IsRequired().HasMaxLength(50).HasDefaultValue("All");
        builder.Property(x => x.DisplayOrder).HasDefaultValue(0);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
    }
}
