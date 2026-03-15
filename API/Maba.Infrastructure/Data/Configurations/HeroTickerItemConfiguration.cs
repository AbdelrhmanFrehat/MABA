using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.HeroTicker;

namespace Maba.Infrastructure.Data.Configurations;

public class HeroTickerItemConfiguration : IEntityTypeConfiguration<HeroTickerItem>
{
    public void Configure(EntityTypeBuilder<HeroTickerItem> builder)
    {
        builder.ToTable("HeroTickerItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(500);
        builder.Property(x => x.ImageUrl).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.SortOrder).HasDefaultValue(0);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
    }
}
