using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Pricing;

namespace Maba.Infrastructure.Data.Configurations;

public class PriceListConfiguration : IEntityTypeConfiguration<PriceList>
{
    public void Configure(EntityTypeBuilder<PriceList> builder)
    {
        builder.ToTable("PriceLists");
        builder.ConfigureBaseEntity<PriceList>();

        builder.Property(x => x.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Currency).HasMaxLength(3).HasDefaultValue("ILS");

        builder.HasOne(x => x.PriceListType)
            .WithMany()
            .HasForeignKey(x => x.PriceListTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PriceListItemConfiguration : IEntityTypeConfiguration<PriceListItem>
{
    public void Configure(EntityTypeBuilder<PriceListItem> builder)
    {
        builder.ToTable("PriceListItems");
        builder.ConfigureBaseEntity<PriceListItem>();

        builder.HasIndex(x => new { x.PriceListId, x.ItemId, x.UnitOfMeasureId, x.MinQuantity }).IsUnique();

        builder.HasOne(x => x.PriceList)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.PriceListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.UnitOfMeasure)
            .WithMany()
            .HasForeignKey(x => x.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
